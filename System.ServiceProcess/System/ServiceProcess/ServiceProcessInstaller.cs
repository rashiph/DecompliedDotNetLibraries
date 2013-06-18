namespace System.ServiceProcess
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Runtime.InteropServices;
    using System.ServiceProcess.Design;
    using System.Text;

    public class ServiceProcessInstaller : ComponentInstaller
    {
        private bool haveLoginInfo;
        private static bool helpPrinted;
        private string password;
        private ServiceAccount serviceAccount = ServiceAccount.User;
        private string username;

        private static bool AccountHasRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            IntPtr zero = IntPtr.Zero;
            int rightsCount = 0;
            int ntStatus = System.ServiceProcess.NativeMethods.LsaEnumerateAccountRights(policyHandle, accountSid, out zero, out rightsCount);
            if (ntStatus == -1073741772)
            {
                return false;
            }
            if (ntStatus != 0)
            {
                throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(ntStatus));
            }
            try
            {
                IntPtr ptr = zero;
                for (int i = 0; i < rightsCount; i++)
                {
                    System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING_withPointer structure = new System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING_withPointer();
                    Marshal.PtrToStructure(ptr, structure);
                    char[] destination = new char[structure.length / 2];
                    Marshal.Copy(structure.pwstr, destination, 0, destination.Length);
                    string strA = new string(destination, 0, destination.Length);
                    if (string.Compare(strA, rightName, StringComparison.Ordinal) == 0)
                    {
                        return true;
                    }
                    ptr = (IntPtr) (((long) ptr) + Marshal.SizeOf(typeof(System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING)));
                }
            }
            finally
            {
                SafeNativeMethods.LsaFreeMemory(zero);
            }
            return false;
        }

        public override void CopyFromComponent(IComponent comp)
        {
        }

        private byte[] GetAccountSid(string accountName)
        {
            byte[] sid = new byte[0x100];
            int[] sidLen = new int[] { sid.Length };
            char[] refDomainName = new char[0x400];
            int[] domNameLen = new int[] { refDomainName.Length };
            int[] sidNameUse = new int[1];
            if (accountName.Substring(0, 2) == @".\")
            {
                StringBuilder lpBuffer = new StringBuilder(0x20);
                int nSize = 0x20;
                if (!System.ServiceProcess.NativeMethods.GetComputerName(lpBuffer, ref nSize))
                {
                    throw new Win32Exception();
                }
                accountName = lpBuffer + accountName.Substring(1);
            }
            if (!System.ServiceProcess.NativeMethods.LookupAccountName(null, accountName, sid, sidLen, refDomainName, domNameLen, sidNameUse))
            {
                throw new Win32Exception();
            }
            byte[] destinationArray = new byte[sidLen[0]];
            Array.Copy(sid, 0, destinationArray, 0, sidLen[0]);
            return destinationArray;
        }

        private void GetLoginInfo()
        {
            if (((base.Context != null) && !base.DesignMode) && !this.haveLoginInfo)
            {
                this.haveLoginInfo = true;
                if (this.serviceAccount == ServiceAccount.User)
                {
                    if (base.Context.Parameters.ContainsKey("username"))
                    {
                        this.username = base.Context.Parameters["username"];
                    }
                    if (base.Context.Parameters.ContainsKey("password"))
                    {
                        this.password = base.Context.Parameters["password"];
                    }
                    if (((this.username == null) || (this.username.Length == 0)) || (this.password == null))
                    {
                        if (!base.Context.Parameters.ContainsKey("unattended"))
                        {
                            using (ServiceInstallerDialog dialog = new ServiceInstallerDialog())
                            {
                                if (this.username != null)
                                {
                                    dialog.Username = this.username;
                                }
                                dialog.ShowDialog();
                                switch (dialog.Result)
                                {
                                    case ServiceInstallerDialogResult.OK:
                                        this.username = dialog.Username;
                                        this.password = dialog.Password;
                                        break;

                                    case ServiceInstallerDialogResult.UseSystem:
                                        this.username = null;
                                        this.password = null;
                                        this.serviceAccount = ServiceAccount.LocalSystem;
                                        break;

                                    case ServiceInstallerDialogResult.Canceled:
                                        throw new InvalidOperationException(System.ServiceProcess.Res.GetString("UserCanceledInstall", new object[] { base.Context.Parameters["assemblypath"] }));
                                }
                                return;
                            }
                        }
                        throw new InvalidOperationException(System.ServiceProcess.Res.GetString("UnattendedCannotPrompt", new object[] { base.Context.Parameters["assemblypath"] }));
                    }
                }
            }
        }

        private static void GrantAccountRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING lsa_unicode_string;
            lsa_unicode_string = new System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING {
                buffer = rightName,
                length = (short) (lsa_unicode_string.buffer.Length * 2),
                maximumLength = lsa_unicode_string.length
            };
            int ntStatus = System.ServiceProcess.NativeMethods.LsaAddAccountRights(policyHandle, accountSid, lsa_unicode_string, 1);
            if (ntStatus != 0)
            {
                throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(ntStatus));
            }
        }

        public override void Install(IDictionary stateSaver)
        {
            try
            {
                ServiceInstaller.CheckEnvironment();
                try
                {
                    if (!this.haveLoginInfo)
                    {
                        try
                        {
                            this.GetLoginInfo();
                        }
                        catch
                        {
                            stateSaver["hadServiceLogonRight"] = true;
                            throw;
                        }
                    }
                }
                finally
                {
                    stateSaver["Account"] = this.Account;
                    if (this.Account == ServiceAccount.User)
                    {
                        stateSaver["Username"] = this.Username;
                    }
                }
                if (this.Account == ServiceAccount.User)
                {
                    IntPtr policyHandle = this.OpenSecurityPolicy();
                    bool flag = true;
                    try
                    {
                        byte[] accountSid = this.GetAccountSid(this.Username);
                        flag = AccountHasRight(policyHandle, accountSid, "SeServiceLogonRight");
                        if (!flag)
                        {
                            GrantAccountRight(policyHandle, accountSid, "SeServiceLogonRight");
                        }
                    }
                    finally
                    {
                        stateSaver["hadServiceLogonRight"] = flag;
                        SafeNativeMethods.LsaClose(policyHandle);
                    }
                }
            }
            finally
            {
                base.Install(stateSaver);
            }
        }

        private IntPtr OpenSecurityPolicy()
        {
            IntPtr ptr3;
            System.ServiceProcess.NativeMethods.LSA_OBJECT_ATTRIBUTES lsa_object_attributes = new System.ServiceProcess.NativeMethods.LSA_OBJECT_ATTRIBUTES();
            GCHandle handle = GCHandle.Alloc(lsa_object_attributes, GCHandleType.Pinned);
            try
            {
                IntPtr ptr;
                int ntStatus = 0;
                IntPtr pointerObjectAttributes = handle.AddrOfPinnedObject();
                ntStatus = System.ServiceProcess.NativeMethods.LsaOpenPolicy(null, pointerObjectAttributes, 0x810, out ptr);
                if (ntStatus != 0)
                {
                    throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(ntStatus));
                }
                ptr3 = ptr;
            }
            finally
            {
                handle.Free();
            }
            return ptr3;
        }

        private static void RemoveAccountRight(IntPtr policyHandle, byte[] accountSid, string rightName)
        {
            System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING lsa_unicode_string;
            lsa_unicode_string = new System.ServiceProcess.NativeMethods.LSA_UNICODE_STRING {
                buffer = rightName,
                length = (short) (lsa_unicode_string.buffer.Length * 2),
                maximumLength = lsa_unicode_string.length
            };
            int ntStatus = System.ServiceProcess.NativeMethods.LsaRemoveAccountRights(policyHandle, accountSid, false, lsa_unicode_string, 1);
            if (ntStatus != 0)
            {
                throw new Win32Exception(SafeNativeMethods.LsaNtStatusToWinError(ntStatus));
            }
        }

        public override void Rollback(IDictionary savedState)
        {
            try
            {
                if ((((ServiceAccount) savedState["Account"]) == ServiceAccount.User) && !((bool) savedState["hadServiceLogonRight"]))
                {
                    string accountName = (string) savedState["Username"];
                    IntPtr policyHandle = this.OpenSecurityPolicy();
                    try
                    {
                        byte[] accountSid = this.GetAccountSid(accountName);
                        RemoveAccountRight(policyHandle, accountSid, "SeServiceLogonRight");
                    }
                    finally
                    {
                        SafeNativeMethods.LsaClose(policyHandle);
                    }
                }
            }
            finally
            {
                base.Rollback(savedState);
            }
        }

        [DefaultValue(3), ServiceProcessDescription("ServiceProcessInstallerAccount")]
        public ServiceAccount Account
        {
            get
            {
                if (!this.haveLoginInfo)
                {
                    this.GetLoginInfo();
                }
                return this.serviceAccount;
            }
            set
            {
                this.haveLoginInfo = false;
                this.serviceAccount = value;
            }
        }

        public override string HelpText
        {
            get
            {
                if (helpPrinted)
                {
                    return base.HelpText;
                }
                helpPrinted = true;
                return (System.ServiceProcess.Res.GetString("HelpText") + "\r\n" + base.HelpText);
            }
        }

        [Browsable(false)]
        public string Password
        {
            get
            {
                if (!this.haveLoginInfo)
                {
                    this.GetLoginInfo();
                }
                return this.password;
            }
            set
            {
                this.haveLoginInfo = false;
                this.password = value;
            }
        }

        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), Browsable(false)]
        public string Username
        {
            get
            {
                if (!this.haveLoginInfo)
                {
                    this.GetLoginInfo();
                }
                return this.username;
            }
            set
            {
                this.haveLoginInfo = false;
                this.username = value;
            }
        }
    }
}

