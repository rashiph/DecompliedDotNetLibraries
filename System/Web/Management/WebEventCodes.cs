namespace System.Web.Management
{
    using System;
    using System.Runtime.InteropServices;

    public sealed class WebEventCodes
    {
        public const int ApplicationCodeBase = 0x3e8;
        internal const int ApplicationCodeBaseLast = 0x3ed;
        public const int ApplicationCompilationEnd = 0x3ec;
        public const int ApplicationCompilationStart = 0x3eb;
        public const int ApplicationDetailCodeBase = 0xc350;
        public const int ApplicationHeartbeat = 0x3ed;
        public const int ApplicationShutdown = 0x3ea;
        public const int ApplicationShutdownBinDirChangeOrDirectoryRename = 0xc357;
        public const int ApplicationShutdownBrowsersDirChangeOrDirectoryRename = 0xc358;
        public const int ApplicationShutdownBuildManagerChange = 0xc361;
        public const int ApplicationShutdownChangeInGlobalAsax = 0xc353;
        public const int ApplicationShutdownChangeInSecurityPolicyFile = 0xc356;
        public const int ApplicationShutdownCodeDirChangeOrDirectoryRename = 0xc359;
        public const int ApplicationShutdownConfigurationChange = 0xc354;
        public const int ApplicationShutdownHostingEnvironment = 0xc352;
        public const int ApplicationShutdownHttpRuntimeClose = 0xc35d;
        public const int ApplicationShutdownIdleTimeout = 0xc35b;
        public const int ApplicationShutdownInitializationError = 0xc35e;
        public const int ApplicationShutdownMaxRecompilationsReached = 0xc35f;
        public const int ApplicationShutdownPhysicalApplicationPathChanged = 0xc35c;
        public const int ApplicationShutdownResourcesDirChangeOrDirectoryRename = 0xc35a;
        public const int ApplicationShutdownUnknown = 0xc351;
        public const int ApplicationShutdownUnloadAppDomainCalled = 0xc355;
        public const int ApplicationStart = 0x3e9;
        public const int AuditCodeBase = 0xfa0;
        internal const int AuditCodeBaseLast = 0xfab;
        public const int AuditDetailCodeBase = 0xc418;
        public const int AuditFileAuthorizationFailure = 0xfa8;
        public const int AuditFileAuthorizationSuccess = 0xfa4;
        public const int AuditFormsAuthenticationFailure = 0xfa5;
        public const int AuditFormsAuthenticationSuccess = 0xfa1;
        public const int AuditInvalidViewStateFailure = 0xfa9;
        public const int AuditMembershipAuthenticationFailure = 0xfa6;
        public const int AuditMembershipAuthenticationSuccess = 0xfa2;
        public const int AuditUnhandledAccessException = 0xfab;
        public const int AuditUnhandledSecurityException = 0xfaa;
        public const int AuditUrlAuthorizationFailure = 0xfa7;
        public const int AuditUrlAuthorizationSuccess = 0xfa3;
        public const int ErrorCodeBase = 0xbb8;
        internal const int ErrorCodeBaseLast = 0xbc3;
        public const int ExpiredTicketFailure = 0xc41a;
        public const int InvalidEventCode = -1;
        public const int InvalidTicketFailure = 0xc419;
        public const int InvalidViewState = 0xc41c;
        public const int InvalidViewStateMac = 0xc41b;
        internal const int LastCodeBase = 0x1770;
        public const int MiscCodeBase = 0x1770;
        internal const int MiscCodeBaseLast = 0x1771;
        public const int RequestCodeBase = 0x7d0;
        internal const int RequestCodeBaseLast = 0x7d2;
        public const int RequestTransactionAbort = 0x7d2;
        public const int RequestTransactionComplete = 0x7d1;
        public const int RuntimeErrorPostTooLarge = 0xbbc;
        public const int RuntimeErrorRequestAbort = 0xbb9;
        public const int RuntimeErrorUnhandledException = 0xbbd;
        public const int RuntimeErrorValidationFailure = 0xbbb;
        public const int RuntimeErrorViewStateFailure = 0xbba;
        internal static int[] s_eventArrayDimensionSizes = new int[2];
        public const int SqlProviderEventsDropped = 0xc47d;
        public const int StateServerConnectionError = 0xc360;
        public const int UndefinedEventCode = 0;
        public const int UndefinedEventDetailCode = 0;
        public const int WebErrorCompilationError = 0xbbf;
        public const int WebErrorConfigurationError = 0xbc0;
        public const int WebErrorObjectStateFormatterDeserializationError = 0xbc3;
        public const int WebErrorOtherError = 0xbc1;
        public const int WebErrorParserError = 0xbbe;
        public const int WebErrorPropertyDeserializationError = 0xbc2;
        public const int WebEventDetailCodeBase = 0xc47c;
        public const int WebEventProviderInformation = 0x1771;
        public const int WebExtendedBase = 0x186a0;

        static WebEventCodes()
        {
            InitEventArrayDimensions();
        }

        private WebEventCodes()
        {
        }

        internal static int GetEventArrayDimensionSize(int dim)
        {
            return s_eventArrayDimensionSizes[dim];
        }

        internal static void GetEventArrayIndexsFromEventCode(int eventCode, out int index0, out int index1)
        {
            index0 = (eventCode / 0x3e8) - 1;
            index1 = (eventCode - ((eventCode / 0x3e8) * 0x3e8)) - 1;
        }

        private static void InitEventArrayDimensions()
        {
            int num = 0;
            int num2 = 5;
            if (num2 > num)
            {
                num = num2;
            }
            num2 = 2;
            if (num2 > num)
            {
                num = num2;
            }
            num2 = 11;
            if (num2 > num)
            {
                num = num2;
            }
            num2 = 11;
            if (num2 > num)
            {
                num = num2;
            }
            num2 = 1;
            if (num2 > num)
            {
                num = num2;
            }
            s_eventArrayDimensionSizes[0] = 6;
            s_eventArrayDimensionSizes[1] = num;
        }

        internal static string MessageFromEventCode(int eventCode, int eventDetailCode)
        {
            string str = null;
            string str2 = null;
            if (eventDetailCode != 0)
            {
                switch (eventDetailCode)
                {
                    case 0xc351:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownUnknown");
                        break;

                    case 0xc352:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownHostingEnvironment");
                        break;

                    case 0xc353:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownChangeInGlobalAsax");
                        break;

                    case 0xc354:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownConfigurationChange");
                        break;

                    case 0xc355:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownUnloadAppDomainCalled");
                        break;

                    case 0xc356:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownChangeInSecurityPolicyFile");
                        break;

                    case 0xc357:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownBinDirChangeOrDirectoryRename");
                        break;

                    case 0xc358:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownBrowsersDirChangeOrDirectoryRename");
                        break;

                    case 0xc359:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownCodeDirChangeOrDirectoryRename");
                        break;

                    case 0xc35a:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownResourcesDirChangeOrDirectoryRename");
                        break;

                    case 0xc35b:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownIdleTimeout");
                        break;

                    case 0xc35c:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownPhysicalApplicationPathChanged");
                        break;

                    case 0xc35d:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownHttpRuntimeClose");
                        break;

                    case 0xc35e:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownInitializationError");
                        break;

                    case 0xc35f:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownMaxRecompilationsReached");
                        break;

                    case 0xc360:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_StateServerConnectionError");
                        break;

                    case 0xc361:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ApplicationShutdownBuildManagerChange");
                        break;

                    case 0xc419:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_InvalidTicketFailure");
                        break;

                    case 0xc41a:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_ExpiredTicketFailure");
                        break;

                    case 0xc41b:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_InvalidViewStateMac");
                        break;

                    case 0xc41c:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_InvalidViewState");
                        break;

                    case 0xc47d:
                        str2 = WebBaseEvent.FormatResourceStringWithCache("Webevent_detail_SqlProviderEventsDropped");
                        break;
                }
            }
            switch (eventCode)
            {
                case 0x3e9:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_ApplicationStart");
                    break;

                case 0x3ea:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_ApplicationShutdown");
                    break;

                case 0x3eb:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_ApplicationCompilationStart");
                    break;

                case 0x3ec:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_ApplicationCompilationEnd");
                    break;

                case 0x3ed:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_ApplicationHeartbeat");
                    break;

                case 0x7d1:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RequestTransactionComplete");
                    break;

                case 0x7d2:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RequestTransactionAbort");
                    break;

                case 0xbb9:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RuntimeErrorRequestAbort");
                    break;

                case 0xbba:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RuntimeErrorViewStateFailure");
                    break;

                case 0xbbb:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RuntimeErrorValidationFailure");
                    break;

                case 0xbbc:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RuntimeErrorPostTooLarge");
                    break;

                case 0xbbd:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_RuntimeErrorUnhandledException");
                    break;

                case 0xbbe:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_WebErrorParserError");
                    break;

                case 0xbbf:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_WebErrorCompilationError");
                    break;

                case 0xbc0:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_WebErrorConfigurationError");
                    break;

                case 0xfa1:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditFormsAuthenticationSuccess");
                    break;

                case 0xfa2:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditMembershipAuthenticationSuccess");
                    break;

                case 0xfa3:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditUrlAuthorizationSuccess");
                    break;

                case 0xfa4:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditFileAuthorizationSuccess");
                    break;

                case 0xfa5:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditFormsAuthenticationFailure");
                    break;

                case 0xfa6:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditMembershipAuthenticationFailure");
                    break;

                case 0xfa7:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditUrlAuthorizationFailure");
                    break;

                case 0xfa8:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditFileAuthorizationFailure");
                    break;

                case 0xfa9:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditInvalidViewStateFailure");
                    break;

                case 0xfaa:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditUnhandledSecurityException");
                    break;

                case 0xfab:
                    str = WebBaseEvent.FormatResourceStringWithCache("Webevent_msg_AuditUnhandledAccessException");
                    break;

                default:
                    return string.Empty;
            }
            if (str2 != null)
            {
                str = str + " " + str2;
            }
            return str;
        }
    }
}

