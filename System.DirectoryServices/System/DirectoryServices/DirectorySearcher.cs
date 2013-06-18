namespace System.DirectoryServices
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.DirectoryServices.Interop;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DSDescription("DirectorySearcherDesc"), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class DirectorySearcher : Component
    {
        private string assertDefaultNamingContext;
        private bool asynchronous;
        private string attributeScopeQuery;
        private bool attributeScopeQuerySpecified;
        private bool cacheResults;
        private bool cacheResultsSpecified;
        private TimeSpan clientTimeout;
        private const string defaultFilter = "(objectClass=*)";
        private DereferenceAlias derefAlias;
        internal bool directorySynchronizationSpecified;
        internal bool directoryVirtualListViewSpecified;
        private bool disposed;
        private System.DirectoryServices.ExtendedDN extendedDN;
        private string filter;
        private static readonly TimeSpan minusOneSecond = new TimeSpan(0, 0, -1);
        private int pageSize;
        private StringCollection propertiesToLoad;
        private bool propertyNamesOnly;
        private ReferralChasingOption referralChasing;
        private bool rootEntryAllocated;
        private System.DirectoryServices.SearchScope scope;
        private bool scopeSpecified;
        internal SearchResultCollection searchResult;
        private DirectoryEntry searchRoot;
        private System.DirectoryServices.SecurityMasks securityMask;
        private TimeSpan serverPageTimeLimit;
        private TimeSpan serverTimeLimit;
        private int sizeLimit;
        private SortOption sort;
        private System.DirectoryServices.DirectorySynchronization sync;
        private bool tombstone;
        private DirectoryVirtualListView vlv;

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher() : this(null, "(objectClass=*)", null, System.DirectoryServices.SearchScope.Subtree)
        {
            this.scopeSpecified = false;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(DirectoryEntry searchRoot) : this(searchRoot, "(objectClass=*)", null, System.DirectoryServices.SearchScope.Subtree)
        {
            this.scopeSpecified = false;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(string filter) : this(null, filter, null, System.DirectoryServices.SearchScope.Subtree)
        {
            this.scopeSpecified = false;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(DirectoryEntry searchRoot, string filter) : this(searchRoot, filter, null, System.DirectoryServices.SearchScope.Subtree)
        {
            this.scopeSpecified = false;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(string filter, string[] propertiesToLoad) : this(null, filter, propertiesToLoad, System.DirectoryServices.SearchScope.Subtree)
        {
            this.scopeSpecified = false;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad) : this(searchRoot, filter, propertiesToLoad, System.DirectoryServices.SearchScope.Subtree)
        {
            this.scopeSpecified = false;
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(string filter, string[] propertiesToLoad, System.DirectoryServices.SearchScope scope) : this(null, filter, propertiesToLoad, scope)
        {
        }

        [DirectoryServicesPermission(SecurityAction.Demand, Unrestricted=true)]
        public DirectorySearcher(DirectoryEntry searchRoot, string filter, string[] propertiesToLoad, System.DirectoryServices.SearchScope scope)
        {
            this.filter = "(objectClass=*)";
            this.scope = System.DirectoryServices.SearchScope.Subtree;
            this.serverTimeLimit = minusOneSecond;
            this.clientTimeout = minusOneSecond;
            this.serverPageTimeLimit = minusOneSecond;
            this.referralChasing = ReferralChasingOption.External;
            this.sort = new SortOption();
            this.cacheResults = true;
            this.attributeScopeQuery = "";
            this.extendedDN = System.DirectoryServices.ExtendedDN.None;
            this.searchRoot = searchRoot;
            this.filter = filter;
            if (propertiesToLoad != null)
            {
                this.PropertiesToLoad.AddRange(propertiesToLoad);
            }
            this.SearchScope = scope;
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                if (this.rootEntryAllocated)
                {
                    this.searchRoot.Dispose();
                }
                this.rootEntryAllocated = false;
                this.disposed = true;
            }
            base.Dispose(disposing);
        }

        private static void DoSetSearchPrefs(System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch adsSearch, AdsSearchPreferenceInfo[] prefs)
        {
            int num = Marshal.SizeOf(typeof(AdsSearchPreferenceInfo));
            IntPtr pSearchPrefs = Marshal.AllocHGlobal((IntPtr) (num * prefs.Length));
            try
            {
                IntPtr ptr = pSearchPrefs;
                for (int i = 0; i < prefs.Length; i++)
                {
                    Marshal.StructureToPtr(prefs[i], ptr, false);
                    ptr = (IntPtr) (((long) ptr) + num);
                }
                adsSearch.SetSearchPreference(pSearchPrefs, prefs.Length);
                ptr = pSearchPrefs;
                for (int j = 0; j < prefs.Length; j++)
                {
                    if (Marshal.ReadInt32(ptr, 0x20) != 0)
                    {
                        int dwSearchPref = prefs[j].dwSearchPref;
                        string str = "";
                        switch (dwSearchPref)
                        {
                            case 0:
                                str = "Asynchronous";
                                break;

                            case 1:
                                str = "DerefAlias";
                                break;

                            case 2:
                                str = "SizeLimit";
                                break;

                            case 3:
                                str = "ServerTimeLimit";
                                break;

                            case 4:
                                str = "PropertyNamesOnly";
                                break;

                            case 5:
                                str = "SearchScope";
                                break;

                            case 6:
                                str = "ClientTimeout";
                                break;

                            case 7:
                                str = "PageSize";
                                break;

                            case 8:
                                str = "ServerPageTimeLimit";
                                break;

                            case 9:
                                str = "ReferralChasing";
                                break;

                            case 10:
                                str = "Sort";
                                break;

                            case 11:
                                str = "CacheResults";
                                break;

                            case 12:
                                str = "DirectorySynchronization";
                                break;

                            case 13:
                                str = "Tombstone";
                                break;

                            case 14:
                                str = "VirtualListView";
                                break;

                            case 15:
                                str = "AttributeScopeQuery";
                                break;

                            case 0x10:
                                str = "SecurityMasks";
                                break;

                            case 0x11:
                                str = "DirectorySynchronizationFlag";
                                break;

                            case 0x12:
                                str = "ExtendedDn";
                                break;
                        }
                        throw new InvalidOperationException(Res.GetString("DSSearchPreferencesNotAccepted", new object[] { str }));
                    }
                    ptr = (IntPtr) (((long) ptr) + num);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pSearchPrefs);
            }
        }

        public SearchResultCollection FindAll()
        {
            return this.FindAll(true);
        }

        private SearchResultCollection FindAll(bool findMoreThanOne)
        {
            IntPtr ptr;
            this.searchResult = null;
            DirectoryEntry root = null;
            if (this.assertDefaultNamingContext == null)
            {
                root = this.SearchRoot.CloneBrowsable();
            }
            else
            {
                root = this.SearchRoot.CloneBrowsable();
            }
            System.DirectoryServices.Interop.UnsafeNativeMethods.IAds adsObject = root.AdsObject;
            if (!(adsObject is System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch))
            {
                throw new NotSupportedException(Res.GetString("DSSearchUnsupported", new object[] { this.SearchRoot.Path }));
            }
            if (this.directoryVirtualListViewSpecified)
            {
                this.SearchRoot.Bind(true);
            }
            System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch adsSearch = (System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch) adsObject;
            this.SetSearchPreferences(adsSearch, findMoreThanOne);
            string[] array = null;
            if (this.PropertiesToLoad.Count > 0)
            {
                if (!this.PropertiesToLoad.Contains("ADsPath"))
                {
                    this.PropertiesToLoad.Add("ADsPath");
                }
                array = new string[this.PropertiesToLoad.Count];
                this.PropertiesToLoad.CopyTo(array, 0);
            }
            if (array != null)
            {
                adsSearch.ExecuteSearch(this.Filter, array, array.Length, out ptr);
            }
            else
            {
                adsSearch.ExecuteSearch(this.Filter, null, -1, out ptr);
                array = new string[0];
            }
            SearchResultCollection results = new SearchResultCollection(root, ptr, array, this);
            this.searchResult = results;
            return results;
        }

        public SearchResult FindOne()
        {
            SearchResult result = null;
            SearchResultCollection results = this.FindAll(false);
            try
            {
                foreach (SearchResult result2 in results)
                {
                    if (this.directorySynchronizationSpecified)
                    {
                        System.DirectoryServices.DirectorySynchronization directorySynchronization = this.DirectorySynchronization;
                    }
                    if (this.directoryVirtualListViewSpecified)
                    {
                        DirectoryVirtualListView virtualListView = this.VirtualListView;
                    }
                    return result2;
                }
                return result;
            }
            finally
            {
                this.searchResult = null;
                results.Dispose();
            }
            return result;
        }

        private unsafe void SetSearchPreferences(System.DirectoryServices.Interop.UnsafeNativeMethods.IDirectorySearch adsSearch, bool findMoreThanOne)
        {
            ArrayList list = new ArrayList();
            AdsSearchPreferenceInfo info = new AdsSearchPreferenceInfo {
                dwSearchPref = 5,
                vValue = new AdsValueHelper((int) this.SearchScope).GetStruct()
            };
            list.Add(info);
            if ((this.sizeLimit != 0) || !findMoreThanOne)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 2,
                    vValue = new AdsValueHelper(findMoreThanOne ? this.SizeLimit : 1).GetStruct()
                };
                list.Add(info);
            }
            if (this.ServerTimeLimit >= new TimeSpan(0L))
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 3,
                    vValue = new AdsValueHelper((int) this.ServerTimeLimit.TotalSeconds).GetStruct()
                };
                list.Add(info);
            }
            info = new AdsSearchPreferenceInfo {
                dwSearchPref = 4,
                vValue = new AdsValueHelper(this.PropertyNamesOnly).GetStruct()
            };
            list.Add(info);
            if (this.ClientTimeout >= new TimeSpan(0L))
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 6,
                    vValue = new AdsValueHelper((int) this.ClientTimeout.TotalSeconds).GetStruct()
                };
                list.Add(info);
            }
            if (this.PageSize != 0)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 7,
                    vValue = new AdsValueHelper(this.PageSize).GetStruct()
                };
                list.Add(info);
            }
            if (this.ServerPageTimeLimit >= new TimeSpan(0L))
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 8,
                    vValue = new AdsValueHelper((int) this.ServerPageTimeLimit.TotalSeconds).GetStruct()
                };
                list.Add(info);
            }
            info = new AdsSearchPreferenceInfo {
                dwSearchPref = 9,
                vValue = new AdsValueHelper((int) this.ReferralChasing).GetStruct()
            };
            list.Add(info);
            if (this.Asynchronous)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 0,
                    vValue = new AdsValueHelper(this.Asynchronous).GetStruct()
                };
                list.Add(info);
            }
            if (this.Tombstone)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 13,
                    vValue = new AdsValueHelper(this.Tombstone).GetStruct()
                };
                list.Add(info);
            }
            if (this.attributeScopeQuerySpecified)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 15,
                    vValue = new AdsValueHelper(this.AttributeScopeQuery, AdsType.ADSTYPE_CASE_IGNORE_STRING).GetStruct()
                };
                list.Add(info);
            }
            if (this.DerefAlias != DereferenceAlias.Never)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 1,
                    vValue = new AdsValueHelper((int) this.DerefAlias).GetStruct()
                };
                list.Add(info);
            }
            if (this.SecurityMasks != System.DirectoryServices.SecurityMasks.None)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 0x10,
                    vValue = new AdsValueHelper((int) this.SecurityMasks).GetStruct()
                };
                list.Add(info);
            }
            if (this.ExtendedDN != System.DirectoryServices.ExtendedDN.None)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 0x12,
                    vValue = new AdsValueHelper((int) this.ExtendedDN).GetStruct()
                };
                list.Add(info);
            }
            if (this.directorySynchronizationSpecified)
            {
                info = new AdsSearchPreferenceInfo {
                    dwSearchPref = 12,
                    vValue = new AdsValueHelper(this.DirectorySynchronization.GetDirectorySynchronizationCookie(), AdsType.ADSTYPE_PROV_SPECIFIC).GetStruct()
                };
                list.Add(info);
                if (this.DirectorySynchronization.Option != DirectorySynchronizationOptions.None)
                {
                    info = new AdsSearchPreferenceInfo {
                        dwSearchPref = 0x11,
                        vValue = new AdsValueHelper((int) this.DirectorySynchronization.Option).GetStruct()
                    };
                    list.Add(info);
                }
            }
            IntPtr zero = IntPtr.Zero;
            IntPtr ptr = IntPtr.Zero;
            IntPtr contextID = IntPtr.Zero;
            try
            {
                if ((this.Sort.PropertyName != null) && (this.Sort.PropertyName.Length > 0))
                {
                    info = new AdsSearchPreferenceInfo {
                        dwSearchPref = 10
                    };
                    AdsSortKey structure = new AdsSortKey {
                        pszAttrType = Marshal.StringToCoTaskMemUni(this.Sort.PropertyName)
                    };
                    zero = structure.pszAttrType;
                    structure.pszReserved = IntPtr.Zero;
                    structure.fReverseOrder = (this.Sort.Direction == SortDirection.Descending) ? -1 : 0;
                    byte[] destination = new byte[Marshal.SizeOf(structure)];
                    Marshal.Copy((IntPtr) ((ulong) ((IntPtr) &structure)), destination, 0, destination.Length);
                    info.vValue = new AdsValueHelper(destination, AdsType.ADSTYPE_PROV_SPECIFIC).GetStruct();
                    list.Add(info);
                }
                if (this.directoryVirtualListViewSpecified)
                {
                    info = new AdsSearchPreferenceInfo {
                        dwSearchPref = 14
                    };
                    AdsVLV svlv = new AdsVLV {
                        beforeCount = this.vlv.BeforeCount,
                        afterCount = this.vlv.AfterCount,
                        offset = this.vlv.Offset
                    };
                    if (this.vlv.Target.Length != 0)
                    {
                        svlv.target = Marshal.StringToCoTaskMemUni(this.vlv.Target);
                    }
                    else
                    {
                        svlv.target = IntPtr.Zero;
                    }
                    ptr = svlv.target;
                    if (this.vlv.DirectoryVirtualListViewContext == null)
                    {
                        svlv.contextIDlength = 0;
                        svlv.contextID = IntPtr.Zero;
                    }
                    else
                    {
                        svlv.contextIDlength = this.vlv.DirectoryVirtualListViewContext.context.Length;
                        svlv.contextID = Marshal.AllocCoTaskMem(svlv.contextIDlength);
                        contextID = svlv.contextID;
                        Marshal.Copy(this.vlv.DirectoryVirtualListViewContext.context, 0, svlv.contextID, svlv.contextIDlength);
                    }
                    IntPtr ptr4 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(AdsVLV)));
                    byte[] buffer2 = new byte[Marshal.SizeOf(svlv)];
                    try
                    {
                        Marshal.StructureToPtr(svlv, ptr4, false);
                        Marshal.Copy(ptr4, buffer2, 0, buffer2.Length);
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(ptr4);
                    }
                    info.vValue = new AdsValueHelper(buffer2, AdsType.ADSTYPE_PROV_SPECIFIC).GetStruct();
                    list.Add(info);
                }
                if (this.cacheResultsSpecified)
                {
                    info = new AdsSearchPreferenceInfo {
                        dwSearchPref = 11,
                        vValue = new AdsValueHelper(this.CacheResults).GetStruct()
                    };
                    list.Add(info);
                }
                AdsSearchPreferenceInfo[] prefs = new AdsSearchPreferenceInfo[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    prefs[i] = (AdsSearchPreferenceInfo) list[i];
                }
                DoSetSearchPrefs(adsSearch, prefs);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(zero);
                }
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(ptr);
                }
                if (contextID != IntPtr.Zero)
                {
                    Marshal.FreeCoTaskMem(contextID);
                }
            }
        }

        [ComVisible(false), DSDescription("DSAsynchronous"), DefaultValue(false)]
        public bool Asynchronous
        {
            get
            {
                return this.asynchronous;
            }
            set
            {
                this.asynchronous = value;
            }
        }

        [DefaultValue(""), ComVisible(false), DSDescription("DSAttributeQuery"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string AttributeScopeQuery
        {
            get
            {
                return this.attributeScopeQuery;
            }
            set
            {
                if (value == null)
                {
                    value = "";
                }
                if (value.Length != 0)
                {
                    if (this.scopeSpecified && (this.SearchScope != System.DirectoryServices.SearchScope.Base))
                    {
                        throw new ArgumentException(Res.GetString("DSBadASQSearchScope"));
                    }
                    this.scope = System.DirectoryServices.SearchScope.Base;
                    this.attributeScopeQuerySpecified = true;
                }
                else
                {
                    this.attributeScopeQuerySpecified = false;
                }
                this.attributeScopeQuery = value;
            }
        }

        [DSDescription("DSCacheResults"), DefaultValue(true)]
        public bool CacheResults
        {
            get
            {
                return this.cacheResults;
            }
            set
            {
                if (this.directoryVirtualListViewSpecified && value)
                {
                    throw new ArgumentException(Res.GetString("DSBadCacheResultsVLV"));
                }
                this.cacheResults = value;
                this.cacheResultsSpecified = true;
            }
        }

        [DSDescription("DSClientTimeout")]
        public TimeSpan ClientTimeout
        {
            get
            {
                return this.clientTimeout;
            }
            set
            {
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
                }
                this.clientTimeout = value;
            }
        }

        [ComVisible(false), DSDescription("DSDerefAlias"), DefaultValue(0)]
        public DereferenceAlias DerefAlias
        {
            get
            {
                return this.derefAlias;
            }
            set
            {
                if ((value < DereferenceAlias.Never) || (value > DereferenceAlias.Always))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(DereferenceAlias));
                }
                this.derefAlias = value;
            }
        }

        [ComVisible(false), DefaultValue((string) null), DSDescription("DSDirectorySynchronization"), Browsable(false)]
        public System.DirectoryServices.DirectorySynchronization DirectorySynchronization
        {
            get
            {
                if (this.directorySynchronizationSpecified && (this.searchResult != null))
                {
                    this.sync.ResetDirectorySynchronizationCookie(this.searchResult.DirsyncCookie);
                }
                return this.sync;
            }
            set
            {
                if (value != null)
                {
                    if (this.PageSize != 0)
                    {
                        throw new ArgumentException(Res.GetString("DSBadPageSizeDirsync"));
                    }
                    this.directorySynchronizationSpecified = true;
                }
                else
                {
                    this.directorySynchronizationSpecified = false;
                }
                this.sync = value;
            }
        }

        [DefaultValue(-1), DSDescription("DSExtendedDn"), ComVisible(false)]
        public System.DirectoryServices.ExtendedDN ExtendedDN
        {
            get
            {
                return this.extendedDN;
            }
            set
            {
                if ((value < System.DirectoryServices.ExtendedDN.None) || (value > System.DirectoryServices.ExtendedDN.Standard))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.ExtendedDN));
                }
                this.extendedDN = value;
            }
        }

        [DSDescription("DSFilter"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SettingsBindable(true), DefaultValue("(objectClass=*)")]
        public string Filter
        {
            get
            {
                return this.filter;
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    value = "(objectClass=*)";
                }
                this.filter = value;
            }
        }

        [DefaultValue(0), DSDescription("DSPageSize")]
        public int PageSize
        {
            get
            {
                return this.pageSize;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadPageSize"));
                }
                if (this.directorySynchronizationSpecified && (value != 0))
                {
                    throw new ArgumentException(Res.GetString("DSBadPageSizeDirsync"));
                }
                this.pageSize = value;
            }
        }

        [DSDescription("DSPropertiesToLoad"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Editor("System.Windows.Forms.Design.StringCollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StringCollection PropertiesToLoad
        {
            get
            {
                if (this.propertiesToLoad == null)
                {
                    this.propertiesToLoad = new StringCollection();
                }
                return this.propertiesToLoad;
            }
        }

        [DSDescription("DSPropertyNamesOnly"), DefaultValue(false)]
        public bool PropertyNamesOnly
        {
            get
            {
                return this.propertyNamesOnly;
            }
            set
            {
                this.propertyNamesOnly = value;
            }
        }

        [DSDescription("DSReferralChasing"), DefaultValue(0x40)]
        public ReferralChasingOption ReferralChasing
        {
            get
            {
                return this.referralChasing;
            }
            set
            {
                if (((value != ReferralChasingOption.None) && (value != ReferralChasingOption.Subordinate)) && ((value != ReferralChasingOption.External) && (value != ReferralChasingOption.All)))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ReferralChasingOption));
                }
                this.referralChasing = value;
            }
        }

        [DefaultValue((string) null), DSDescription("DSSearchRoot")]
        public DirectoryEntry SearchRoot
        {
            get
            {
                if ((this.searchRoot == null) && !base.DesignMode)
                {
                    DirectoryEntry entry = new DirectoryEntry("LDAP://RootDSE", true, null, null, AuthenticationTypes.Secure);
                    string str = (string) entry.Properties["defaultNamingContext"][0];
                    entry.Dispose();
                    this.searchRoot = new DirectoryEntry("LDAP://" + str, true, null, null, AuthenticationTypes.Secure);
                    this.rootEntryAllocated = true;
                    this.assertDefaultNamingContext = "LDAP://" + str;
                }
                return this.searchRoot;
            }
            set
            {
                if (this.rootEntryAllocated)
                {
                    this.searchRoot.Dispose();
                }
                this.rootEntryAllocated = false;
                this.assertDefaultNamingContext = null;
                this.searchRoot = value;
            }
        }

        [SettingsBindable(true), DSDescription("DSSearchScope"), DefaultValue(2)]
        public System.DirectoryServices.SearchScope SearchScope
        {
            get
            {
                return this.scope;
            }
            set
            {
                if ((value < System.DirectoryServices.SearchScope.Base) || (value > System.DirectoryServices.SearchScope.Subtree))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.SearchScope));
                }
                if (this.attributeScopeQuerySpecified && (value != System.DirectoryServices.SearchScope.Base))
                {
                    throw new ArgumentException(Res.GetString("DSBadASQSearchScope"));
                }
                this.scope = value;
                this.scopeSpecified = true;
            }
        }

        [DSDescription("DSSecurityMasks"), ComVisible(false), DefaultValue(0)]
        public System.DirectoryServices.SecurityMasks SecurityMasks
        {
            get
            {
                return this.securityMask;
            }
            set
            {
                if (value > (System.DirectoryServices.SecurityMasks.Sacl | System.DirectoryServices.SecurityMasks.Dacl | System.DirectoryServices.SecurityMasks.Group | System.DirectoryServices.SecurityMasks.Owner))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.SecurityMasks));
                }
                this.securityMask = value;
            }
        }

        [DSDescription("DSServerPageTimeLimit")]
        public TimeSpan ServerPageTimeLimit
        {
            get
            {
                return this.serverPageTimeLimit;
            }
            set
            {
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
                }
                this.serverPageTimeLimit = value;
            }
        }

        [DSDescription("DSServerTimeLimit")]
        public TimeSpan ServerTimeLimit
        {
            get
            {
                return this.serverTimeLimit;
            }
            set
            {
                if (value.TotalSeconds > 2147483647.0)
                {
                    throw new ArgumentException(Res.GetString("TimespanExceedMax"), "value");
                }
                this.serverTimeLimit = value;
            }
        }

        [DefaultValue(0), DSDescription("DSSizeLimit")]
        public int SizeLimit
        {
            get
            {
                return this.sizeLimit;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadSizeLimit"));
                }
                this.sizeLimit = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), TypeConverter(typeof(ExpandableObjectConverter)), DSDescription("DSSort")]
        public SortOption Sort
        {
            get
            {
                return this.sort;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.sort = value;
            }
        }

        [ComVisible(false), DSDescription("DSTombstone"), DefaultValue(false)]
        public bool Tombstone
        {
            get
            {
                return this.tombstone;
            }
            set
            {
                this.tombstone = value;
            }
        }

        [Browsable(false), DSDescription("DSVirtualListView"), ComVisible(false), DefaultValue((string) null)]
        public DirectoryVirtualListView VirtualListView
        {
            get
            {
                if (this.directoryVirtualListViewSpecified && (this.searchResult != null))
                {
                    DirectoryVirtualListView vLVResponse = this.searchResult.VLVResponse;
                    this.vlv.Offset = vLVResponse.Offset;
                    this.vlv.ApproximateTotal = vLVResponse.ApproximateTotal;
                    this.vlv.DirectoryVirtualListViewContext = vLVResponse.DirectoryVirtualListViewContext;
                    if (this.vlv.ApproximateTotal != 0)
                    {
                        this.vlv.TargetPercentage = (int) ((((double) this.vlv.Offset) / ((double) this.vlv.ApproximateTotal)) * 100.0);
                    }
                    else
                    {
                        this.vlv.TargetPercentage = 0;
                    }
                }
                return this.vlv;
            }
            set
            {
                if (value != null)
                {
                    if (this.cacheResultsSpecified && this.CacheResults)
                    {
                        throw new ArgumentException(Res.GetString("DSBadCacheResultsVLV"));
                    }
                    this.directoryVirtualListViewSpecified = true;
                    this.cacheResults = false;
                }
                else
                {
                    this.directoryVirtualListViewSpecified = false;
                }
                this.vlv = value;
            }
        }
    }
}

