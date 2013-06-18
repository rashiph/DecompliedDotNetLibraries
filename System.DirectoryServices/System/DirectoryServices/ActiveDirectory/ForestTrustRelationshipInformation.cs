namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ForestTrustRelationshipInformation : TrustRelationshipInformation
    {
        private ArrayList binaryData = new ArrayList();
        private ArrayList binaryDataTime = new ArrayList();
        private ForestTrustDomainInfoCollection domainInfo = new ForestTrustDomainInfoCollection();
        private StringCollection excludedNames = new StringCollection();
        private Hashtable excludedNameTime = new Hashtable();
        internal bool retrieved;
        private TopLevelNameCollection topLevelNames = new TopLevelNameCollection();

        internal ForestTrustRelationshipInformation(DirectoryContext context, string source, DS_DOMAIN_TRUSTS unmanagedTrust, TrustType type)
        {
            string str = null;
            string str2 = null;
            base.context = context;
            base.source = source;
            if (unmanagedTrust.DnsDomainName != IntPtr.Zero)
            {
                str = Marshal.PtrToStringUni(unmanagedTrust.DnsDomainName);
            }
            if (unmanagedTrust.NetbiosDomainName != IntPtr.Zero)
            {
                str2 = Marshal.PtrToStringUni(unmanagedTrust.NetbiosDomainName);
            }
            base.target = (str == null) ? str2 : str;
            if (((unmanagedTrust.Flags & 2) != 0) && ((unmanagedTrust.Flags & 0x20) != 0))
            {
                base.direction = TrustDirection.Bidirectional;
            }
            else if ((unmanagedTrust.Flags & 2) != 0)
            {
                base.direction = TrustDirection.Outbound;
            }
            else if ((unmanagedTrust.Flags & 0x20) != 0)
            {
                base.direction = TrustDirection.Inbound;
            }
            base.type = type;
        }

        private void GetForestTrustInfoHelper()
        {
            IntPtr zero = IntPtr.Zero;
            PolicySafeHandle handle = null;
            LSA_UNICODE_STRING result = null;
            bool flag = false;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            TopLevelNameCollection names = new TopLevelNameCollection();
            StringCollection strings = new StringCollection();
            ForestTrustDomainInfoCollection infos = new ForestTrustDomainInfoCollection();
            ArrayList list = new ArrayList();
            Hashtable hashtable = new Hashtable();
            ArrayList list2 = new ArrayList();
            try
            {
                try
                {
                    result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(base.TargetName);
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    serverName = Utils.GetPolicyServerName(base.context, true, false, base.source);
                    flag = Utils.Impersonate(base.context);
                    handle = new PolicySafeHandle(Utils.GetPolicyHandle(serverName));
                    int status = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaQueryForestTrustInformation(handle, result, ref zero);
                    if (status != 0)
                    {
                        int errorCode = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError(status);
                        if (errorCode != 0)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
                        }
                    }
                    try
                    {
                        if (zero != IntPtr.Zero)
                        {
                            LSA_FOREST_TRUST_INFORMATION structure = new LSA_FOREST_TRUST_INFORMATION();
                            Marshal.PtrToStructure(zero, structure);
                            int recordCount = structure.RecordCount;
                            IntPtr ptr = IntPtr.Zero;
                            for (int i = 0; i < recordCount; i++)
                            {
                                ptr = Marshal.ReadIntPtr(structure.Entries, i * Marshal.SizeOf(typeof(IntPtr)));
                                LSA_FOREST_TRUST_RECORD lsa_forest_trust_record = new LSA_FOREST_TRUST_RECORD();
                                Marshal.PtrToStructure(ptr, lsa_forest_trust_record);
                                if (lsa_forest_trust_record.ForestTrustType == LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelName)
                                {
                                    IntPtr ptr4 = (IntPtr) (((long) ptr) + 0x10L);
                                    Marshal.PtrToStructure(ptr4, lsa_forest_trust_record.TopLevelName);
                                    TopLevelName name = new TopLevelName(lsa_forest_trust_record.Flags, lsa_forest_trust_record.TopLevelName, lsa_forest_trust_record.Time);
                                    names.Add(name);
                                }
                                else if (lsa_forest_trust_record.ForestTrustType == LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelNameEx)
                                {
                                    IntPtr ptr5 = (IntPtr) (((long) ptr) + 0x10L);
                                    Marshal.PtrToStructure(ptr5, lsa_forest_trust_record.TopLevelName);
                                    string str2 = Marshal.PtrToStringUni(lsa_forest_trust_record.TopLevelName.Buffer, lsa_forest_trust_record.TopLevelName.Length / 2);
                                    strings.Add(str2);
                                    hashtable.Add(str2, lsa_forest_trust_record.Time);
                                }
                                else if (lsa_forest_trust_record.ForestTrustType == LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustDomainInfo)
                                {
                                    ForestTrustDomainInformation info = new ForestTrustDomainInformation(lsa_forest_trust_record.Flags, lsa_forest_trust_record.DomainInfo, lsa_forest_trust_record.Time);
                                    infos.Add(info);
                                }
                                else if (lsa_forest_trust_record.ForestTrustType != LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustRecordTypeLast)
                                {
                                    int length = lsa_forest_trust_record.Data.Length;
                                    byte[] destination = new byte[length];
                                    if ((lsa_forest_trust_record.Data.Buffer != IntPtr.Zero) && (length != 0))
                                    {
                                        Marshal.Copy(lsa_forest_trust_record.Data.Buffer, destination, 0, length);
                                    }
                                    list.Add(destination);
                                    list2.Add(lsa_forest_trust_record.Time);
                                }
                            }
                        }
                    }
                    finally
                    {
                        System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaFreeMemory(zero);
                    }
                    this.topLevelNames = names;
                    this.excludedNames = strings;
                    this.domainInfo = infos;
                    this.binaryData = list;
                    this.excludedNameTime = hashtable;
                    this.binaryDataTime = list2;
                    this.retrieved = true;
                }
                finally
                {
                    if (flag)
                    {
                        Utils.Revert();
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public void Save()
        {
            int num = 0;
            IntPtr zero = IntPtr.Zero;
            int num2 = 0;
            IntPtr ptr2 = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            PolicySafeHandle handle = null;
            IntPtr collisionInfo = IntPtr.Zero;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            bool flag = false;
            IntPtr s = IntPtr.Zero;
            string serverName = null;
            IntPtr fileTime = IntPtr.Zero;
            int count = this.TopLevelNames.Count;
            int num4 = this.ExcludedTopLevelNames.Count;
            int num5 = this.TrustedDomainInformation.Count;
            int num6 = 0;
            num += count;
            num += num4;
            num += num5;
            if (this.binaryData.Count != 0)
            {
                num6 = this.binaryData.Count;
                num++;
                num += num6;
            }
            zero = Marshal.AllocHGlobal((int) (num * Marshal.SizeOf(typeof(IntPtr))));
            try
            {
                try
                {
                    IntPtr ptr7 = IntPtr.Zero;
                    fileTime = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(FileTime)));
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.GetSystemTimeAsFileTime(fileTime);
                    FileTime structure = new FileTime();
                    Marshal.PtrToStructure(fileTime, structure);
                    for (int i = 0; i < count; i++)
                    {
                        LSA_FOREST_TRUST_RECORD lsa_forest_trust_record = new LSA_FOREST_TRUST_RECORD {
                            Flags = (int) this.topLevelNames[i].Status,
                            ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelName
                        };
                        TopLevelName name = this.topLevelNames[i];
                        lsa_forest_trust_record.Time = name.time;
                        lsa_forest_trust_record.TopLevelName = new LSA_UNICODE_STRING();
                        ptr7 = Marshal.StringToHGlobalUni(name.Name);
                        list.Add(ptr7);
                        System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RtlInitUnicodeString(lsa_forest_trust_record.TopLevelName, ptr7);
                        ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
                        list.Add(ptr2);
                        Marshal.StructureToPtr(lsa_forest_trust_record, ptr2, false);
                        Marshal.WriteIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2, ptr2);
                        num2++;
                    }
                    for (int j = 0; j < num4; j++)
                    {
                        LSA_FOREST_TRUST_RECORD lsa_forest_trust_record2 = new LSA_FOREST_TRUST_RECORD {
                            Flags = 0,
                            ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustTopLevelNameEx
                        };
                        if (this.excludedNameTime.Contains(this.excludedNames[j]))
                        {
                            lsa_forest_trust_record2.Time = (LARGE_INTEGER) this.excludedNameTime[j];
                        }
                        else
                        {
                            lsa_forest_trust_record2.Time = new LARGE_INTEGER();
                            lsa_forest_trust_record2.Time.lowPart = structure.lower;
                            lsa_forest_trust_record2.Time.highPart = structure.higher;
                        }
                        lsa_forest_trust_record2.TopLevelName = new LSA_UNICODE_STRING();
                        ptr7 = Marshal.StringToHGlobalUni(this.excludedNames[j]);
                        list.Add(ptr7);
                        System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RtlInitUnicodeString(lsa_forest_trust_record2.TopLevelName, ptr7);
                        ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
                        list.Add(ptr2);
                        Marshal.StructureToPtr(lsa_forest_trust_record2, ptr2, false);
                        Marshal.WriteIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2, ptr2);
                        num2++;
                    }
                    for (int k = 0; k < num5; k++)
                    {
                        LSA_FOREST_TRUST_RECORD lsa_forest_trust_record3 = new LSA_FOREST_TRUST_RECORD {
                            Flags = (int) this.domainInfo[k].Status,
                            ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustDomainInfo
                        };
                        ForestTrustDomainInformation information = this.domainInfo[k];
                        lsa_forest_trust_record3.Time = information.time;
                        IntPtr pSid = IntPtr.Zero;
                        IntPtr ptr9 = IntPtr.Zero;
                        ptr9 = Marshal.StringToHGlobalUni(information.DomainSid);
                        list.Add(ptr9);
                        if (System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.ConvertStringSidToSidW(ptr9, ref pSid) == 0)
                        {
                            throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
                        }
                        lsa_forest_trust_record3.DomainInfo = new LSA_FOREST_TRUST_DOMAIN_INFO();
                        lsa_forest_trust_record3.DomainInfo.sid = pSid;
                        list2.Add(pSid);
                        lsa_forest_trust_record3.DomainInfo.DNSNameBuffer = Marshal.StringToHGlobalUni(information.DnsName);
                        list.Add(lsa_forest_trust_record3.DomainInfo.DNSNameBuffer);
                        lsa_forest_trust_record3.DomainInfo.DNSNameLength = (information.DnsName == null) ? ((short) 0) : ((short) (information.DnsName.Length * 2));
                        lsa_forest_trust_record3.DomainInfo.DNSNameMaximumLength = (information.DnsName == null) ? ((short) 0) : ((short) (information.DnsName.Length * 2));
                        lsa_forest_trust_record3.DomainInfo.NetBIOSNameBuffer = Marshal.StringToHGlobalUni(information.NetBiosName);
                        list.Add(lsa_forest_trust_record3.DomainInfo.NetBIOSNameBuffer);
                        lsa_forest_trust_record3.DomainInfo.NetBIOSNameLength = (information.NetBiosName == null) ? ((short) 0) : ((short) (information.NetBiosName.Length * 2));
                        lsa_forest_trust_record3.DomainInfo.NetBIOSNameMaximumLength = (information.NetBiosName == null) ? ((short) 0) : ((short) (information.NetBiosName.Length * 2));
                        ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
                        list.Add(ptr2);
                        Marshal.StructureToPtr(lsa_forest_trust_record3, ptr2, false);
                        Marshal.WriteIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2, ptr2);
                        num2++;
                    }
                    if (num6 > 0)
                    {
                        LSA_FOREST_TRUST_RECORD lsa_forest_trust_record4 = new LSA_FOREST_TRUST_RECORD {
                            Flags = 0,
                            ForestTrustType = LSA_FOREST_TRUST_RECORD_TYPE.ForestTrustRecordTypeLast
                        };
                        ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
                        list.Add(ptr2);
                        Marshal.StructureToPtr(lsa_forest_trust_record4, ptr2, false);
                        Marshal.WriteIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2, ptr2);
                        num2++;
                        for (int m = 0; m < num6; m++)
                        {
                            LSA_FOREST_TRUST_RECORD lsa_forest_trust_record5 = new LSA_FOREST_TRUST_RECORD {
                                Flags = 0,
                                Time = (LARGE_INTEGER) this.binaryDataTime[m]
                            };
                            lsa_forest_trust_record5.Data.Length = ((byte[]) this.binaryData[m]).Length;
                            if (lsa_forest_trust_record5.Data.Length == 0)
                            {
                                lsa_forest_trust_record5.Data.Buffer = IntPtr.Zero;
                            }
                            else
                            {
                                lsa_forest_trust_record5.Data.Buffer = Marshal.AllocHGlobal(lsa_forest_trust_record5.Data.Length);
                                list.Add(lsa_forest_trust_record5.Data.Buffer);
                                Marshal.Copy((byte[]) this.binaryData[m], 0, lsa_forest_trust_record5.Data.Buffer, lsa_forest_trust_record5.Data.Length);
                            }
                            ptr2 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_RECORD)));
                            list.Add(ptr2);
                            Marshal.StructureToPtr(lsa_forest_trust_record5, ptr2, false);
                            Marshal.WriteIntPtr(zero, Marshal.SizeOf(typeof(IntPtr)) * num2, ptr2);
                            num2++;
                        }
                    }
                    LSA_FOREST_TRUST_INFORMATION lsa_forest_trust_information = new LSA_FOREST_TRUST_INFORMATION {
                        RecordCount = num,
                        Entries = zero
                    };
                    ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_FOREST_TRUST_INFORMATION)));
                    Marshal.StructureToPtr(lsa_forest_trust_information, ptr, false);
                    serverName = Utils.GetPolicyServerName(base.context, true, true, base.SourceName);
                    flag = Utils.Impersonate(base.context);
                    handle = new PolicySafeHandle(Utils.GetPolicyHandle(serverName));
                    LSA_UNICODE_STRING result = new LSA_UNICODE_STRING();
                    s = Marshal.StringToHGlobalUni(base.TargetName);
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.RtlInitUnicodeString(result, s);
                    int status = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaSetForestTrustInformation(handle, result, ptr, 1, out collisionInfo);
                    if (status != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaNtStatusToWinError(status), serverName);
                    }
                    if (collisionInfo != IntPtr.Zero)
                    {
                        throw ExceptionHelper.CreateForestTrustCollisionException(collisionInfo);
                    }
                    status = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaSetForestTrustInformation(handle, result, ptr, 0, out collisionInfo);
                    if (status != 0)
                    {
                        throw ExceptionHelper.GetExceptionFromErrorCode(status, serverName);
                    }
                    this.retrieved = false;
                }
                finally
                {
                    if (flag)
                    {
                        Utils.Revert();
                    }
                    for (int n = 0; n < list.Count; n++)
                    {
                        Marshal.FreeHGlobal((IntPtr) list[n]);
                    }
                    for (int num14 = 0; num14 < list2.Count; num14++)
                    {
                        System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LocalFree((IntPtr) list2[num14]);
                    }
                    if (zero != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(zero);
                    }
                    if (ptr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptr);
                    }
                    if (collisionInfo != IntPtr.Zero)
                    {
                        System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.LsaFreeMemory(collisionInfo);
                    }
                    if (s != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(s);
                    }
                    if (fileTime != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(fileTime);
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        public StringCollection ExcludedTopLevelNames
        {
            get
            {
                if (!this.retrieved)
                {
                    this.GetForestTrustInfoHelper();
                }
                return this.excludedNames;
            }
        }

        public TopLevelNameCollection TopLevelNames
        {
            get
            {
                if (!this.retrieved)
                {
                    this.GetForestTrustInfoHelper();
                }
                return this.topLevelNames;
            }
        }

        public ForestTrustDomainInfoCollection TrustedDomainInformation
        {
            get
            {
                if (!this.retrieved)
                {
                    this.GetForestTrustInfoHelper();
                }
                return this.domainInfo;
            }
        }
    }
}

