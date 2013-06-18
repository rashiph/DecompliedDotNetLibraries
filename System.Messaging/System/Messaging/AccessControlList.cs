namespace System.Messaging
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Messaging.Interop;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    public class AccessControlList : CollectionBase
    {
        private static int environment = UnknownEnvironment;
        internal static readonly int NonNtEnvironment = 3;
        internal static readonly int NtEnvironment = 2;
        private static object staticLock = new object();
        internal static readonly int UnknownEnvironment = 0;
        internal static readonly int W2kEnvironment = 1;

        public int Add(AccessControlEntry entry)
        {
            return base.List.Add(entry);
        }

        internal static void CheckEnvironment()
        {
            if (CurrentEnvironment == NonNtEnvironment)
            {
                throw new PlatformNotSupportedException(Res.GetString("WinNTRequired"));
            }
        }

        public bool Contains(AccessControlEntry entry)
        {
            return base.List.Contains(entry);
        }

        public void CopyTo(AccessControlEntry[] array, int index)
        {
            base.List.CopyTo(array, index);
        }

        internal static void FreeAcl(IntPtr acl)
        {
            SafeNativeMethods.LocalFree(acl);
        }

        public int IndexOf(AccessControlEntry entry)
        {
            return base.List.IndexOf(entry);
        }

        public void Insert(int index, AccessControlEntry entry)
        {
            base.List.Insert(index, entry);
        }

        internal IntPtr MakeAcl(IntPtr oldAcl)
        {
            IntPtr ptr;
            CheckEnvironment();
            int count = base.List.Count;
            System.Messaging.Interop.NativeMethods.ExplicitAccess[] accessArray = new System.Messaging.Interop.NativeMethods.ExplicitAccess[count];
            GCHandle handle = GCHandle.Alloc(accessArray, GCHandleType.Pinned);
            try
            {
                for (int i = 0; i < count; i++)
                {
                    int num4;
                    int sidSize = 0;
                    int domainSize = 0;
                    AccessControlEntry entry = (AccessControlEntry) base.List[i];
                    if (entry.Trustee == null)
                    {
                        throw new InvalidOperationException(Res.GetString("InvalidTrustee"));
                    }
                    string name = entry.Trustee.Name;
                    if (name == null)
                    {
                        throw new InvalidOperationException(Res.GetString("InvalidTrusteeName"));
                    }
                    if ((entry.Trustee.TrusteeType == TrusteeType.Computer) && !name.EndsWith("$"))
                    {
                        name = name + "$";
                    }
                    if (!System.Messaging.Interop.UnsafeNativeMethods.LookupAccountName(entry.Trustee.SystemName, name, IntPtr.Zero, ref sidSize, null, ref domainSize, out num4))
                    {
                        int num6 = Marshal.GetLastWin32Error();
                        if (num6 != 0x7a)
                        {
                            throw new InvalidOperationException(Res.GetString("CouldntResolve", new object[] { entry.Trustee.Name, num6 }));
                        }
                    }
                    accessArray[i].data = Marshal.AllocHGlobal(sidSize);
                    StringBuilder domainName = new StringBuilder(domainSize);
                    if (!System.Messaging.Interop.UnsafeNativeMethods.LookupAccountName(entry.Trustee.SystemName, name, accessArray[i].data, ref sidSize, domainName, ref domainSize, out num4))
                    {
                        throw new InvalidOperationException(Res.GetString("CouldntResolveName", new object[] { entry.Trustee.Name }));
                    }
                    accessArray[i].grfAccessPermissions = entry.accessFlags;
                    accessArray[i].grfAccessMode = (int) entry.EntryType;
                    accessArray[i].grfInheritance = 0;
                    accessArray[i].pMultipleTrustees = IntPtr.Zero;
                    accessArray[i].MultipleTrusteeOperation = 0;
                    accessArray[i].TrusteeForm = 0;
                    accessArray[i].TrusteeType = (int) entry.Trustee.TrusteeType;
                }
                int error = SafeNativeMethods.SetEntriesInAclW(count, handle.AddrOfPinnedObject(), oldAcl, out ptr);
                if (error != 0)
                {
                    throw new Win32Exception(error);
                }
            }
            finally
            {
                handle.Free();
                for (int j = 0; j < count; j++)
                {
                    if (accessArray[j].data != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(accessArray[j].data);
                    }
                }
            }
            return ptr;
        }

        public void Remove(AccessControlEntry entry)
        {
            base.List.Remove(entry);
        }

        internal static int CurrentEnvironment
        {
            get
            {
                if (environment == UnknownEnvironment)
                {
                    lock (staticLock)
                    {
                        if (environment == UnknownEnvironment)
                        {
                            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                            try
                            {
                                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                                {
                                    if (Environment.OSVersion.Version.Major >= 5)
                                    {
                                        environment = W2kEnvironment;
                                    }
                                    else
                                    {
                                        environment = NtEnvironment;
                                    }
                                }
                                else
                                {
                                    environment = NonNtEnvironment;
                                }
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                }
                return environment;
            }
        }
    }
}

