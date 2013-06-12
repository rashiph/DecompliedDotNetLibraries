namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.ExceptionServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    [Serializable, ComVisible(true)]
    public class WindowsIdentity : IIdentity, ISerializable, IDeserializationCallback, IDisposable
    {
        private string m_authType;
        private object m_groups;
        private TokenImpersonationLevel? m_impersonationLevel;
        private int m_isAuthenticated;
        private string m_name;
        private SecurityIdentifier m_owner;
        [SecurityCritical]
        private SafeTokenHandle m_safeTokenHandle;
        private SecurityIdentifier m_user;
        [SecurityCritical]
        private static SafeTokenHandle s_invalidTokenHandle = SafeTokenHandle.InvalidHandle;

        [SecurityCritical]
        private WindowsIdentity()
        {
            this.m_safeTokenHandle = SafeTokenHandle.InvalidHandle;
            this.m_isAuthenticated = -1;
        }

        [SecurityCritical]
        internal WindowsIdentity(SafeTokenHandle safeTokenHandle) : this(safeTokenHandle.DangerousGetHandle(), null, -1)
        {
            GC.KeepAlive(safeTokenHandle);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(IntPtr userToken) : this(userToken, null, -1)
        {
        }

        [SecurityCritical]
        private WindowsIdentity(SerializationInfo info)
        {
            this.m_safeTokenHandle = SafeTokenHandle.InvalidHandle;
            this.m_isAuthenticated = -1;
            IntPtr userToken = (IntPtr) info.GetValue("m_userToken", typeof(IntPtr));
            if (userToken != IntPtr.Zero)
            {
                this.CreateFromToken(userToken);
            }
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(string sUserPrincipalName) : this(sUserPrincipalName, null)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(IntPtr userToken, string type) : this(userToken, type, -1)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(SerializationInfo info, StreamingContext context) : this(info)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(string sUserPrincipalName, string type)
        {
            this.m_safeTokenHandle = SafeTokenHandle.InvalidHandle;
            this.m_isAuthenticated = -1;
            KerbS4ULogon(sUserPrincipalName, ref this.m_safeTokenHandle);
        }

        [SecurityCritical]
        private WindowsIdentity(IntPtr userToken, string authType, int isAuthenticated)
        {
            this.m_safeTokenHandle = SafeTokenHandle.InvalidHandle;
            this.m_isAuthenticated = -1;
            this.CreateFromToken(userToken);
            this.m_authType = authType;
            this.m_isAuthenticated = isAuthenticated;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType) : this(userToken, type, -1)
        {
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public WindowsIdentity(IntPtr userToken, string type, WindowsAccountType acctType, bool isAuthenticated) : this(userToken, type, isAuthenticated ? 1 : 0)
        {
        }

        [SecurityCritical]
        private void CreateFromToken(IntPtr userToken)
        {
            if (userToken == IntPtr.Zero)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_TokenZero"));
            }
            uint returnLength = (uint) Marshal.SizeOf(typeof(uint));
            Win32Native.GetTokenInformation(userToken, 8, SafeLocalAllocHandle.InvalidHandle, 0, out returnLength);
            if (Marshal.GetLastWin32Error() == 6)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
            }
            if (!Win32Native.DuplicateHandle(Win32Native.GetCurrentProcess(), userToken, Win32Native.GetCurrentProcess(), ref this.m_safeTokenHandle, 0, true, 2))
            {
                throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
            }
        }

        [SecuritySafeCritical, ComVisible(false)]
        public void Dispose()
        {
            this.Dispose(true);
        }

        [ComVisible(false), SecuritySafeCritical]
        protected virtual void Dispose(bool disposing)
        {
            if ((disposing && (this.m_safeTokenHandle != null)) && !this.m_safeTokenHandle.IsClosed)
            {
                this.m_safeTokenHandle.Dispose();
            }
            this.m_name = null;
            this.m_owner = null;
            this.m_user = null;
        }

        [SecuritySafeCritical]
        public static WindowsIdentity GetAnonymous()
        {
            return new WindowsIdentity();
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public static WindowsIdentity GetCurrent()
        {
            return GetCurrentInternal(TokenAccessLevels.MaximumAllowed, false);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public static WindowsIdentity GetCurrent(bool ifImpersonating)
        {
            return GetCurrentInternal(TokenAccessLevels.MaximumAllowed, ifImpersonating);
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal)]
        public static WindowsIdentity GetCurrent(TokenAccessLevels desiredAccess)
        {
            return GetCurrentInternal(desiredAccess, false);
        }

        [SecurityCritical]
        internal static WindowsIdentity GetCurrentInternal(TokenAccessLevels desiredAccess, bool threadOnly)
        {
            bool flag;
            int hr = 0;
            SafeTokenHandle handle = GetCurrentToken(desiredAccess, threadOnly, out flag, out hr);
            if ((handle == null) || handle.IsInvalid)
            {
                if (!threadOnly || flag)
                {
                    throw new SecurityException(Win32Native.GetMessage(hr));
                }
                return null;
            }
            WindowsIdentity identity = new WindowsIdentity();
            identity.m_safeTokenHandle.Dispose();
            identity.m_safeTokenHandle = handle;
            return identity;
        }

        [SecurityCritical]
        private static SafeTokenHandle GetCurrentProcessToken(TokenAccessLevels desiredAccess, out int hr)
        {
            SafeTokenHandle handle;
            hr = 0;
            if (!Win32Native.OpenProcessToken(Win32Native.GetCurrentProcess(), desiredAccess, out handle))
            {
                hr = GetHRForWin32Error(Marshal.GetLastWin32Error());
            }
            return handle;
        }

        [SecurityCritical]
        internal static SafeTokenHandle GetCurrentThreadToken(TokenAccessLevels desiredAccess, out int hr)
        {
            SafeTokenHandle handle;
            hr = System.Security.Principal.Win32.OpenThreadToken(desiredAccess, WinSecurityContext.Both, out handle);
            return handle;
        }

        [SecurityCritical]
        internal static WindowsIdentity GetCurrentThreadWI()
        {
            return SecurityContext.GetCurrentWI(Thread.CurrentThread.GetExecutionContextNoCreate());
        }

        [SecurityCritical]
        private static SafeTokenHandle GetCurrentToken(TokenAccessLevels desiredAccess, bool threadOnly, out bool isImpersonating, out int hr)
        {
            isImpersonating = true;
            SafeTokenHandle currentThreadToken = GetCurrentThreadToken(desiredAccess, out hr);
            if ((currentThreadToken == null) && (hr == GetHRForWin32Error(0x3f0)))
            {
                isImpersonating = false;
                if (!threadOnly)
                {
                    currentThreadToken = GetCurrentProcessToken(desiredAccess, out hr);
                }
            }
            return currentThreadToken;
        }

        [SecurityCritical]
        private static Exception GetExceptionFromNtStatus(int status)
        {
            if (status == -1073741790)
            {
                return new UnauthorizedAccessException();
            }
            if ((status != -1073741670) && (status != -1073741801))
            {
                return new SecurityException(Win32Native.GetMessage(Win32Native.LsaNtStatusToWinError(status)));
            }
            return new OutOfMemoryException();
        }

        private static int GetHRForWin32Error(int dwLastError)
        {
            if ((dwLastError & 0x80000000L) == 0x80000000L)
            {
                return dwLastError;
            }
            return ((dwLastError & 0xffff) | -2147024896);
        }

        [SecurityCritical]
        private static Win32Native.LUID GetLogonAuthId(SafeTokenHandle safeTokenHandle)
        {
            using (SafeLocalAllocHandle handle = GetTokenInformation(safeTokenHandle, TokenInformationClass.TokenStatistics))
            {
                return handle.Read<Win32Native.TOKEN_STATISTICS>(0L).AuthenticationId;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        internal string GetName()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            if (this.m_safeTokenHandle.IsInvalid)
            {
                return string.Empty;
            }
            if (this.m_name == null)
            {
                using (SafeRevertToSelf(ref lookForMyCaller))
                {
                    this.m_name = (this.User.Translate(typeof(NTAccount)) as NTAccount).ToString();
                }
            }
            return this.m_name;
        }

        [SecurityCritical]
        private T GetTokenInformation<T>(TokenInformationClass tokenInformationClass) where T: struct
        {
            using (SafeLocalAllocHandle handle = GetTokenInformation(this.m_safeTokenHandle, tokenInformationClass))
            {
                return handle.Read<T>(0L);
            }
        }

        [SecurityCritical]
        private static SafeLocalAllocHandle GetTokenInformation(SafeTokenHandle tokenHandle, TokenInformationClass tokenInformationClass)
        {
            SafeLocalAllocHandle invalidHandle = SafeLocalAllocHandle.InvalidHandle;
            uint returnLength = (uint) Marshal.SizeOf(typeof(uint));
            bool flag = Win32Native.GetTokenInformation(tokenHandle, (uint) tokenInformationClass, invalidHandle, 0, out returnLength);
            int errorCode = Marshal.GetLastWin32Error();
            int num3 = errorCode;
            if (num3 == 6)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidImpersonationToken"));
            }
            if ((num3 != 0x18) && (num3 != 0x7a))
            {
                throw new SecurityException(Win32Native.GetMessage(errorCode));
            }
            IntPtr sizetdwBytes = new IntPtr((long) returnLength);
            invalidHandle.Dispose();
            invalidHandle = Win32Native.LocalAlloc(0, sizetdwBytes);
            if ((invalidHandle == null) || invalidHandle.IsInvalid)
            {
                throw new OutOfMemoryException();
            }
            invalidHandle.Initialize((ulong) returnLength);
            if (!Win32Native.GetTokenInformation(tokenHandle, (uint) tokenInformationClass, invalidHandle, returnLength, out returnLength))
            {
                throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
            }
            return invalidHandle;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public virtual WindowsImpersonationContext Impersonate()
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            return this.Impersonate(ref lookForMyCaller);
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.UnmanagedCode)]
        public static WindowsImpersonationContext Impersonate(IntPtr userToken)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            if (userToken == IntPtr.Zero)
            {
                return SafeRevertToSelf(ref lookForMyCaller);
            }
            WindowsIdentity identity = new WindowsIdentity(userToken, null, -1);
            return identity.Impersonate(ref lookForMyCaller);
        }

        [SecurityCritical]
        internal WindowsImpersonationContext Impersonate(ref StackCrawlMark stackMark)
        {
            if (this.m_safeTokenHandle.IsInvalid)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AnonymousCannotImpersonate"));
            }
            return SafeImpersonate(this.m_safeTokenHandle, this, ref stackMark);
        }

        [HandleProcessCorruptedStateExceptions, SecurityCritical]
        private static unsafe SafeTokenHandle KerbS4ULogon(string upn, ref SafeTokenHandle safeTokenHandle)
        {
            SafeTokenHandle handle6;
            byte[] array = new byte[] { 0x43, 0x4c, 0x52 };
            IntPtr sizetdwBytes = new IntPtr((long) ((ulong) (array.Length + 1)));
            using (SafeLocalAllocHandle handle = Win32Native.LocalAlloc(0x40, sizetdwBytes))
            {
                if ((handle == null) || handle.IsInvalid)
                {
                    throw new OutOfMemoryException();
                }
                handle.Initialize((ulong) (array.Length + 1L));
                handle.WriteArray<byte>(0L, array, 0, array.Length);
                Win32Native.UNICODE_INTPTR_STRING logonProcessName = new Win32Native.UNICODE_INTPTR_STRING(array.Length, handle);
                SafeLsaLogonProcessHandle invalidHandle = SafeLsaLogonProcessHandle.InvalidHandle;
                SafeLsaReturnBufferHandle profileBuffer = SafeLsaReturnBufferHandle.InvalidHandle;
                try
                {
                    int num;
                    Privilege privilege = null;
                    RuntimeHelpers.PrepareConstrainedRegions();
                    try
                    {
                        try
                        {
                            privilege = new Privilege("SeTcbPrivilege");
                            privilege.Enable();
                        }
                        catch (PrivilegeNotHeldException)
                        {
                        }
                        IntPtr zero = IntPtr.Zero;
                        num = Win32Native.LsaRegisterLogonProcess(ref logonProcessName, ref invalidHandle, ref zero);
                        if (5 == Win32Native.LsaNtStatusToWinError(num))
                        {
                            num = Win32Native.LsaConnectUntrusted(ref invalidHandle);
                        }
                    }
                    catch
                    {
                        if (privilege != null)
                        {
                            privilege.Revert();
                        }
                        throw;
                    }
                    finally
                    {
                        if (privilege != null)
                        {
                            privilege.Revert();
                        }
                    }
                    if (num < 0)
                    {
                        throw GetExceptionFromNtStatus(num);
                    }
                    byte[] bytes = new byte["Kerberos".Length + 1];
                    Encoding.ASCII.GetBytes("Kerberos", 0, "Kerberos".Length, bytes, 0);
                    sizetdwBytes = new IntPtr((long) ((ulong) bytes.Length));
                    using (SafeLocalAllocHandle handle4 = Win32Native.LocalAlloc(0, sizetdwBytes))
                    {
                        if ((handle4 == null) || handle4.IsInvalid)
                        {
                            throw new OutOfMemoryException();
                        }
                        handle4.Initialize((ulong) bytes.Length);
                        handle4.WriteArray<byte>(0L, bytes, 0, bytes.Length);
                        Win32Native.UNICODE_INTPTR_STRING packageName = new Win32Native.UNICODE_INTPTR_STRING("Kerberos".Length, handle4);
                        uint authenticationPackage = 0;
                        num = Win32Native.LsaLookupAuthenticationPackage(invalidHandle, ref packageName, ref authenticationPackage);
                        if (num < 0)
                        {
                            throw GetExceptionFromNtStatus(num);
                        }
                        Win32Native.TOKEN_SOURCE sourceContext = new Win32Native.TOKEN_SOURCE();
                        if (!Win32Native.AllocateLocallyUniqueId(ref sourceContext.SourceIdentifier))
                        {
                            throw new SecurityException(Win32Native.GetMessage(Marshal.GetLastWin32Error()));
                        }
                        sourceContext.Name = new char[8];
                        sourceContext.Name[0] = 'C';
                        sourceContext.Name[1] = 'L';
                        sourceContext.Name[2] = 'R';
                        uint profileBufferLength = 0;
                        Win32Native.LUID logonId = new Win32Native.LUID();
                        Win32Native.QUOTA_LIMITS quotas = new Win32Native.QUOTA_LIMITS();
                        int subStatus = 0;
                        byte[] buffer3 = Encoding.Unicode.GetBytes(upn);
                        int num5 = Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON)) + buffer3.Length;
                        using (SafeLocalAllocHandle handle5 = Win32Native.LocalAlloc(0x40, new IntPtr(num5)))
                        {
                            if ((handle5 == null) || handle5.IsInvalid)
                            {
                                throw new OutOfMemoryException();
                            }
                            handle5.Initialize((ulong) num5);
                            ulong byteOffset = (ulong) Marshal.SizeOf(typeof(Win32Native.KERB_S4U_LOGON));
                            handle5.WriteArray<byte>(byteOffset, buffer3, 0, buffer3.Length);
                            byte* pointer = null;
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try
                            {
                                handle5.AcquirePointer(ref pointer);
                                Win32Native.KERB_S4U_LOGON kerb_su_logon = new Win32Native.KERB_S4U_LOGON {
                                    MessageType = 12,
                                    Flags = 0,
                                    ClientUpn = new Win32Native.UNICODE_INTPTR_STRING(buffer3.Length, new IntPtr((void*) (pointer + byteOffset)))
                                };
                                handle5.Write<Win32Native.KERB_S4U_LOGON>(0L, kerb_su_logon);
                                num = Win32Native.LsaLogonUser(invalidHandle, ref logonProcessName, 3, authenticationPackage, new IntPtr((void*) pointer), (uint) handle5.ByteLength, IntPtr.Zero, ref sourceContext, ref profileBuffer, ref profileBufferLength, ref logonId, ref safeTokenHandle, ref quotas, ref subStatus);
                                if ((num == -1073741714) && (subStatus < 0))
                                {
                                    num = subStatus;
                                }
                                if (num < 0)
                                {
                                    throw GetExceptionFromNtStatus(num);
                                }
                                if (subStatus < 0)
                                {
                                    throw GetExceptionFromNtStatus(subStatus);
                                }
                            }
                            finally
                            {
                                if (pointer != null)
                                {
                                    handle5.ReleasePointer();
                                }
                            }
                        }
                        handle6 = safeTokenHandle;
                    }
                }
                finally
                {
                    if (!invalidHandle.IsInvalid)
                    {
                        invalidHandle.Dispose();
                    }
                    if (!profileBuffer.IsInvalid)
                    {
                        profileBuffer.Dispose();
                    }
                }
            }
            return handle6;
        }

        [SecurityCritical]
        internal static ImpersonationQueryResult QueryImpersonation()
        {
            SafeTokenHandle phThreadToken = null;
            int num = System.Security.Principal.Win32.OpenThreadToken(TokenAccessLevels.Query, WinSecurityContext.Thread, out phThreadToken);
            if (phThreadToken != null)
            {
                phThreadToken.Close();
                return ImpersonationQueryResult.Impersonated;
            }
            if (num == GetHRForWin32Error(5))
            {
                return ImpersonationQueryResult.Impersonated;
            }
            if (num == GetHRForWin32Error(0x3f0))
            {
                return ImpersonationQueryResult.NotImpersonated;
            }
            return ImpersonationQueryResult.Failed;
        }

        [SecurityCritical]
        internal static WindowsImpersonationContext SafeImpersonate(SafeTokenHandle userToken, WindowsIdentity wi, ref StackCrawlMark stackMark)
        {
            bool flag;
            int hr = 0;
            SafeTokenHandle safeTokenHandle = GetCurrentToken(TokenAccessLevels.MaximumAllowed, false, out flag, out hr);
            if ((safeTokenHandle == null) || safeTokenHandle.IsInvalid)
            {
                throw new SecurityException(Win32Native.GetMessage(hr));
            }
            FrameSecurityDescriptor securityObjectForFrame = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, true);
            if (securityObjectForFrame == null)
            {
                throw new SecurityException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            WindowsImpersonationContext context = new WindowsImpersonationContext(safeTokenHandle, GetCurrentThreadWI(), flag, securityObjectForFrame);
            if (userToken.IsInvalid)
            {
                hr = System.Security.Principal.Win32.RevertToSelf();
                if (hr < 0)
                {
                    Environment.FailFast(Win32Native.GetMessage(hr));
                }
                UpdateThreadWI(wi);
                securityObjectForFrame.SetTokenHandles(safeTokenHandle, (wi == null) ? null : wi.TokenHandle);
                return context;
            }
            hr = System.Security.Principal.Win32.RevertToSelf();
            if (hr < 0)
            {
                Environment.FailFast(Win32Native.GetMessage(hr));
            }
            if (System.Security.Principal.Win32.ImpersonateLoggedOnUser(userToken) < 0)
            {
                context.Undo();
                throw new SecurityException(Environment.GetResourceString("Argument_ImpersonateUser"));
            }
            UpdateThreadWI(wi);
            securityObjectForFrame.SetTokenHandles(safeTokenHandle, (wi == null) ? null : wi.TokenHandle);
            return context;
        }

        [SecurityCritical]
        internal static WindowsImpersonationContext SafeRevertToSelf(ref StackCrawlMark stackMark)
        {
            return SafeImpersonate(s_invalidTokenHandle, null, ref stackMark);
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("m_userToken", this.m_safeTokenHandle.DangerousGetHandle());
        }

        internal static void UpdateThreadWI(WindowsIdentity wi)
        {
            SecurityContext currentSecurityContextNoCreate = SecurityContext.GetCurrentSecurityContextNoCreate();
            if ((wi != null) && (currentSecurityContextNoCreate == null))
            {
                currentSecurityContextNoCreate = new SecurityContext();
                Thread.CurrentThread.ExecutionContext.SecurityContext = currentSecurityContextNoCreate;
            }
            if (currentSecurityContextNoCreate != null)
            {
                currentSecurityContextNoCreate.WindowsIdentity = wi;
            }
        }

        public string AuthenticationType
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return string.Empty;
                }
                if (this.m_authType == null)
                {
                    Win32Native.LUID logonAuthId = GetLogonAuthId(this.m_safeTokenHandle);
                    if (logonAuthId.LowPart == 0x3e6)
                    {
                        return string.Empty;
                    }
                    SafeLsaReturnBufferHandle invalidHandle = SafeLsaReturnBufferHandle.InvalidHandle;
                    try
                    {
                        int status = Win32Native.LsaGetLogonSessionData(ref logonAuthId, ref invalidHandle);
                        if (status < 0)
                        {
                            throw GetExceptionFromNtStatus(status);
                        }
                        invalidHandle.Initialize((ulong) Marshal.SizeOf(typeof(Win32Native.SECURITY_LOGON_SESSION_DATA)));
                        return Marshal.PtrToStringUni(invalidHandle.Read<Win32Native.SECURITY_LOGON_SESSION_DATA>(0L).AuthenticationPackage.Buffer);
                    }
                    finally
                    {
                        if (!invalidHandle.IsInvalid)
                        {
                            invalidHandle.Dispose();
                        }
                    }
                }
                return this.m_authType;
            }
        }

        public IdentityReferenceCollection Groups
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return null;
                }
                if (this.m_groups == null)
                {
                    IdentityReferenceCollection references = new IdentityReferenceCollection();
                    using (SafeLocalAllocHandle handle = GetTokenInformation(this.m_safeTokenHandle, TokenInformationClass.TokenGroups))
                    {
                        if (handle.Read<uint>(0L) != 0)
                        {
                            Win32Native.SID_AND_ATTRIBUTES[] array = new Win32Native.SID_AND_ATTRIBUTES[handle.Read<Win32Native.TOKEN_GROUPS>(0L).GroupCount];
                            handle.ReadArray<Win32Native.SID_AND_ATTRIBUTES>((ulong) Marshal.OffsetOf(typeof(Win32Native.TOKEN_GROUPS), "Groups").ToInt32(), array, 0, array.Length);
                            foreach (Win32Native.SID_AND_ATTRIBUTES sid_and_attributes in array)
                            {
                                uint num2 = 0xc0000014;
                                if ((sid_and_attributes.Attributes & num2) == 4)
                                {
                                    references.Add(new SecurityIdentifier(sid_and_attributes.Sid, true));
                                }
                            }
                        }
                    }
                    Interlocked.CompareExchange(ref this.m_groups, references, null);
                }
                return (this.m_groups as IdentityReferenceCollection);
            }
        }

        [ComVisible(false)]
        public TokenImpersonationLevel ImpersonationLevel
        {
            [SecuritySafeCritical]
            get
            {
                if (!this.m_impersonationLevel.HasValue)
                {
                    this.m_impersonationLevel = 0;
                    if (this.m_safeTokenHandle.IsInvalid)
                    {
                        this.m_impersonationLevel = 1;
                    }
                    else if (this.GetTokenInformation<int>(TokenInformationClass.TokenType) == 1)
                    {
                        this.m_impersonationLevel = 0;
                    }
                    else
                    {
                        int tokenInformation = this.GetTokenInformation<int>(TokenInformationClass.TokenImpersonationLevel);
                        this.m_impersonationLevel = new TokenImpersonationLevel?((TokenImpersonationLevel) (tokenInformation + 1));
                    }
                }
                return this.m_impersonationLevel.Value;
            }
        }

        public virtual bool IsAnonymous
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return true;
                }
                SecurityIdentifier identifier = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[] { 7 });
                return (this.User == identifier);
            }
        }

        public virtual bool IsAuthenticated
        {
            get
            {
                if (this.m_isAuthenticated == -1)
                {
                    WindowsPrincipal principal = new WindowsPrincipal(this);
                    SecurityIdentifier sid = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[] { 11 });
                    this.m_isAuthenticated = principal.IsInRole(sid) ? 1 : 0;
                }
                return (this.m_isAuthenticated == 1);
            }
        }

        public virtual bool IsGuest
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return false;
                }
                WindowsPrincipal principal = new WindowsPrincipal(this);
                return principal.IsInRole(WindowsBuiltInRole.Guest);
            }
        }

        public virtual bool IsSystem
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return false;
                }
                SecurityIdentifier identifier = new SecurityIdentifier(IdentifierAuthority.NTAuthority, new int[] { 0x12 });
                return (this.User == identifier);
            }
        }

        public virtual string Name
        {
            [SecuritySafeCritical]
            get
            {
                return this.GetName();
            }
        }

        [ComVisible(false)]
        public SecurityIdentifier Owner
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return null;
                }
                if (this.m_owner == null)
                {
                    using (SafeLocalAllocHandle handle = GetTokenInformation(this.m_safeTokenHandle, TokenInformationClass.TokenOwner))
                    {
                        this.m_owner = new SecurityIdentifier(handle.Read<IntPtr>(0L), true);
                    }
                }
                return this.m_owner;
            }
        }

        public virtual IntPtr Token
        {
            [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this.m_safeTokenHandle.DangerousGetHandle();
            }
        }

        internal SafeTokenHandle TokenHandle
        {
            [SecurityCritical]
            get
            {
                return this.m_safeTokenHandle;
            }
        }

        [ComVisible(false)]
        public SecurityIdentifier User
        {
            [SecuritySafeCritical]
            get
            {
                if (this.m_safeTokenHandle.IsInvalid)
                {
                    return null;
                }
                if (this.m_user == null)
                {
                    using (SafeLocalAllocHandle handle = GetTokenInformation(this.m_safeTokenHandle, TokenInformationClass.TokenUser))
                    {
                        this.m_user = new SecurityIdentifier(handle.Read<IntPtr>(0L), true);
                    }
                }
                return this.m_user;
            }
        }
    }
}

