namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;
    using System.Text;

    internal class ExceptionHelper
    {
        private static int ERROR_ACCESS_DENIED = 5;
        private static int ERROR_CANCELLED = 0x4c7;
        private static int ERROR_DS_DRA_ACCESS_DENIED = 0x2105;
        internal static int ERROR_DS_DRA_BAD_DN = 0x20f7;
        private static int ERROR_DS_DRA_OUT_OF_MEM = 0x20fe;
        internal static int ERROR_DS_NAME_UNPARSEABLE = 0x209e;
        internal static int ERROR_DS_UNKNOWN_ERROR = 0x20ef;
        private static int ERROR_NO_LOGON_SERVERS = 0x51f;
        private static int ERROR_NO_SUCH_DOMAIN = 0x54b;
        private static int ERROR_NOT_ENOUGH_MEMORY = 8;
        private static int ERROR_OUTOFMEMORY = 14;
        internal static int RPC_S_CALL_FAILED = 0x6be;
        private static int RPC_S_OUT_OF_RESOURCES = 0x6b9;
        internal static int RPC_S_SERVER_UNAVAILABLE = 0x6ba;

        internal static Exception CreateForestTrustCollisionException(IntPtr collisionInfo)
        {
            ForestTrustRelationshipCollisionCollection collisions = new ForestTrustRelationshipCollisionCollection();
            LSA_FOREST_TRUST_COLLISION_INFORMATION structure = new LSA_FOREST_TRUST_COLLISION_INFORMATION();
            Marshal.PtrToStructure(collisionInfo, structure);
            int recordCount = structure.RecordCount;
            IntPtr zero = IntPtr.Zero;
            for (int i = 0; i < recordCount; i++)
            {
                zero = Marshal.ReadIntPtr(structure.Entries, i * Marshal.SizeOf(typeof(IntPtr)));
                LSA_FOREST_TRUST_COLLISION_RECORD lsa_forest_trust_collision_record = new LSA_FOREST_TRUST_COLLISION_RECORD();
                Marshal.PtrToStructure(zero, lsa_forest_trust_collision_record);
                ForestTrustCollisionType collisionType = lsa_forest_trust_collision_record.Type;
                string record = Marshal.PtrToStringUni(lsa_forest_trust_collision_record.Name.Buffer, lsa_forest_trust_collision_record.Name.Length / 2);
                TopLevelNameCollisionOptions none = TopLevelNameCollisionOptions.None;
                DomainCollisionOptions domainFlag = DomainCollisionOptions.None;
                switch (collisionType)
                {
                    case ForestTrustCollisionType.TopLevelName:
                        none = (TopLevelNameCollisionOptions) lsa_forest_trust_collision_record.Flags;
                        break;

                    case ForestTrustCollisionType.Domain:
                        domainFlag = (DomainCollisionOptions) lsa_forest_trust_collision_record.Flags;
                        break;
                }
                ForestTrustRelationshipCollision collision = new ForestTrustRelationshipCollision(collisionType, none, domainFlag, record);
                collisions.Add(collision);
            }
            return new ForestTrustCollisionException(Res.GetString("ForestTrustCollision"), null, collisions);
        }

        internal static SyncFromAllServersOperationException CreateSyncAllException(IntPtr errorInfo, bool singleError)
        {
            if (errorInfo == IntPtr.Zero)
            {
                return new SyncFromAllServersOperationException();
            }
            if (singleError)
            {
                DS_REPSYNCALL_ERRINFO structure = new DS_REPSYNCALL_ERRINFO();
                Marshal.PtrToStructure(errorInfo, structure);
                string errorMessage = GetErrorMessage(structure.dwWin32Err, false);
                string sourceServer = Marshal.PtrToStringUni(structure.pszSrcId);
                string targetServer = Marshal.PtrToStringUni(structure.pszSvrId);
                if (structure.dwWin32Err == ERROR_CANCELLED)
                {
                    return null;
                }
                SyncFromAllServersErrorInformation information = new SyncFromAllServersErrorInformation(structure.error, structure.dwWin32Err, errorMessage, sourceServer, targetServer);
                return new SyncFromAllServersOperationException(Res.GetString("DSSyncAllFailure"), null, new SyncFromAllServersErrorInformation[] { information });
            }
            IntPtr ptr = Marshal.ReadIntPtr(errorInfo);
            ArrayList list = new ArrayList();
            int num = 0;
            while (ptr != IntPtr.Zero)
            {
                DS_REPSYNCALL_ERRINFO ds_repsyncall_errinfo2 = new DS_REPSYNCALL_ERRINFO();
                Marshal.PtrToStructure(ptr, ds_repsyncall_errinfo2);
                if (ds_repsyncall_errinfo2.dwWin32Err != ERROR_CANCELLED)
                {
                    string str4 = GetErrorMessage(ds_repsyncall_errinfo2.dwWin32Err, false);
                    string str5 = Marshal.PtrToStringUni(ds_repsyncall_errinfo2.pszSrcId);
                    string str6 = Marshal.PtrToStringUni(ds_repsyncall_errinfo2.pszSvrId);
                    SyncFromAllServersErrorInformation information2 = new SyncFromAllServersErrorInformation(ds_repsyncall_errinfo2.error, ds_repsyncall_errinfo2.dwWin32Err, str4, str5, str6);
                    list.Add(information2);
                }
                num++;
                ptr = Marshal.ReadIntPtr(errorInfo, num * Marshal.SizeOf(typeof(IntPtr)));
            }
            if (list.Count == 0)
            {
                return null;
            }
            SyncFromAllServersErrorInformation[] errors = new SyncFromAllServersErrorInformation[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                SyncFromAllServersErrorInformation information3 = (SyncFromAllServersErrorInformation) list[i];
                errors[i] = new SyncFromAllServersErrorInformation(information3.ErrorCategory, information3.ErrorCode, information3.ErrorMessage, information3.SourceServer, information3.TargetServer);
            }
            return new SyncFromAllServersOperationException(Res.GetString("DSSyncAllFailure"), null, errors);
        }

        internal static string GetErrorMessage(int errorCode, bool hresult)
        {
            uint num = (uint) errorCode;
            if (!hresult)
            {
                num = ((num & 0xffff) | 0x70000) | 0x80000000;
            }
            StringBuilder lpBuffer = new StringBuilder(0x100);
            int length = UnsafeNativeMethods.FormatMessageW(0x3200, 0, (int) num, 0, lpBuffer, lpBuffer.Capacity + 1, 0);
            if (length != 0)
            {
                return lpBuffer.ToString(0, length);
            }
            return Res.GetString("DSUnknown", new object[] { Convert.ToString((long) num, 0x10) });
        }

        internal static Exception GetExceptionFromCOMException(COMException e)
        {
            return GetExceptionFromCOMException(null, e);
        }

        internal static Exception GetExceptionFromCOMException(DirectoryContext context, COMException e)
        {
            int errorCode = e.ErrorCode;
            string message = e.Message;
            switch (errorCode)
            {
                case -2147024891:
                    return new UnauthorizedAccessException(message, e);

                case -2147023570:
                    return new AuthenticationException(message, e);

                case -2147016657:
                    return new InvalidOperationException(message, e);

                case -2147016651:
                    return new InvalidOperationException(message, e);

                case -2147019886:
                    return new ActiveDirectoryObjectExistsException(message, e);

                case -2147024888:
                    return new OutOfMemoryException();

                case -2147016646:
                case -2147016690:
                case -2147016689:
                    if (context != null)
                    {
                        return new ActiveDirectoryServerDownException(message, e, errorCode, context.GetServerName());
                    }
                    return new ActiveDirectoryServerDownException(message, e, errorCode, null);
            }
            return new ActiveDirectoryOperationException(message, e, errorCode);
        }

        internal static Exception GetExceptionFromErrorCode(int errorCode)
        {
            return GetExceptionFromErrorCode(errorCode, null);
        }

        internal static Exception GetExceptionFromErrorCode(int errorCode, string targetName)
        {
            string errorMessage = GetErrorMessage(errorCode, false);
            if ((errorCode == ERROR_ACCESS_DENIED) || (errorCode == ERROR_DS_DRA_ACCESS_DENIED))
            {
                return new UnauthorizedAccessException(errorMessage);
            }
            if (((errorCode == ERROR_NOT_ENOUGH_MEMORY) || (errorCode == ERROR_OUTOFMEMORY)) || ((errorCode == ERROR_DS_DRA_OUT_OF_MEM) || (errorCode == RPC_S_OUT_OF_RESOURCES)))
            {
                return new OutOfMemoryException();
            }
            if (((errorCode != ERROR_NO_LOGON_SERVERS) && (errorCode != ERROR_NO_SUCH_DOMAIN)) && ((errorCode != RPC_S_SERVER_UNAVAILABLE) && (errorCode != RPC_S_CALL_FAILED)))
            {
                return new ActiveDirectoryOperationException(errorMessage, errorCode);
            }
            return new ActiveDirectoryServerDownException(errorMessage, errorCode, targetName);
        }
    }
}

