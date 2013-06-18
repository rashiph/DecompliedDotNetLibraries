namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class Domain : ActiveDirectoryPartition
    {
        private DomainCollection cachedChildren;
        private DomainControllerCollection cachedDomainControllers;
        private System.DirectoryServices.ActiveDirectory.Forest cachedForest;
        private DomainController cachedInfrastructureRoleOwner;
        private Domain cachedParent;
        private DomainController cachedPdcRoleOwner;
        private DomainController cachedRidRoleOwner;
        private string crossRefDN;
        private System.DirectoryServices.ActiveDirectory.DomainMode currentDomainMode;
        private bool isParentInitialized;
        private string trustParent;

        internal Domain(DirectoryContext context, string domainName) : this(context, domainName, new DirectoryEntryManager(context))
        {
        }

        internal Domain(DirectoryContext context, string domainName, DirectoryEntryManager directoryEntryMgr) : base(context, domainName)
        {
            this.currentDomainMode = ~System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000MixedDomain;
            base.directoryEntryMgr = directoryEntryMgr;
        }

        public void CreateLocalSideOfTrustRelationship(string targetDomainName, TrustDirection direction, string trustPassword)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            if ((direction < TrustDirection.Inbound) || (direction > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(TrustDirection));
            }
            if (trustPassword == null)
            {
                throw new ArgumentNullException("trustPassword");
            }
            if (trustPassword.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "trustPassword");
            }
            Locator.GetDomainControllerInfo(null, targetDomainName, null, 0x10L);
            DirectoryContext targetContext = Utils.GetNewDirectoryContext(targetDomainName, DirectoryContextType.Domain, base.context);
            TrustHelper.CreateTrust(base.context, base.Name, targetContext, targetDomainName, false, direction, trustPassword);
        }

        public void CreateTrustRelationship(Domain targetDomain, TrustDirection direction)
        {
            base.CheckIfDisposed();
            if (targetDomain == null)
            {
                throw new ArgumentNullException("targetDomain");
            }
            if ((direction < TrustDirection.Inbound) || (direction > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(TrustDirection));
            }
            string password = TrustHelper.CreateTrustPassword();
            TrustHelper.CreateTrust(base.context, base.Name, targetDomain.GetDirectoryContext(), targetDomain.Name, false, direction, password);
            int num = 0;
            if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                num |= 2;
            }
            if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                num |= 1;
            }
            TrustHelper.CreateTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.context, base.Name, false, (TrustDirection) num, password);
        }

        public void DeleteLocalSideOfTrustRelationship(string targetDomainName)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            TrustHelper.DeleteTrust(base.context, base.Name, targetDomainName, false);
        }

        public void DeleteTrustRelationship(Domain targetDomain)
        {
            base.CheckIfDisposed();
            if (targetDomain == null)
            {
                throw new ArgumentNullException("targetDomain");
            }
            TrustHelper.DeleteTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false);
            TrustHelper.DeleteTrust(base.context, base.Name, targetDomain.Name, false);
        }

        public DomainControllerCollection FindAllDiscoverableDomainControllers()
        {
            long dcFlags = 0x1000L;
            base.CheckIfDisposed();
            return new DomainControllerCollection(Locator.EnumerateDomainControllers(base.context, base.Name, null, dcFlags));
        }

        public DomainControllerCollection FindAllDiscoverableDomainControllers(string siteName)
        {
            long dcFlags = 0x1000L;
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (siteName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            return new DomainControllerCollection(Locator.EnumerateDomainControllers(base.context, base.Name, siteName, dcFlags));
        }

        public DomainControllerCollection FindAllDomainControllers()
        {
            base.CheckIfDisposed();
            return DomainController.FindAllInternal(base.context, base.Name, true, null);
        }

        public DomainControllerCollection FindAllDomainControllers(string siteName)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return DomainController.FindAllInternal(base.context, base.Name, true, siteName);
        }

        public DomainController FindDomainController()
        {
            base.CheckIfDisposed();
            return DomainController.FindOneInternal(base.context, base.Name, null, 0L);
        }

        public DomainController FindDomainController(LocatorOptions flag)
        {
            base.CheckIfDisposed();
            return DomainController.FindOneInternal(base.context, base.Name, null, flag);
        }

        public DomainController FindDomainController(string siteName)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return DomainController.FindOneInternal(base.context, base.Name, siteName, 0L);
        }

        public DomainController FindDomainController(string siteName, LocatorOptions flag)
        {
            base.CheckIfDisposed();
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return DomainController.FindOneInternal(base.context, base.Name, siteName, flag);
        }

        public TrustRelationshipInformationCollection GetAllTrustRelationships()
        {
            base.CheckIfDisposed();
            return new TrustRelationshipInformationCollection(base.context, base.Name, this.GetTrustsHelper(null));
        }

        private ArrayList GetChildDomains()
        {
            ArrayList list = new ArrayList();
            if (this.crossRefDN == null)
            {
                this.LoadCrossRefAttributes();
            }
            DirectoryEntry searchRoot = null;
            SearchResultCollection results = null;
            try
            {
                searchRoot = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
                StringBuilder builder = new StringBuilder(15);
                builder.Append("(&(");
                builder.Append(PropertyManager.ObjectCategory);
                builder.Append("=crossRef)(");
                builder.Append(PropertyManager.SystemFlags);
                builder.Append(":1.2.840.113556.1.4.804:=");
                builder.Append(1);
                builder.Append(")(");
                builder.Append(PropertyManager.SystemFlags);
                builder.Append(":1.2.840.113556.1.4.804:=");
                builder.Append(2);
                builder.Append(")(");
                builder.Append(PropertyManager.TrustParent);
                builder.Append("=");
                builder.Append(Utils.GetEscapedFilterValue(this.crossRefDN));
                builder.Append("))");
                string filter = builder.ToString();
                string[] propertiesToLoad = new string[] { PropertyManager.DnsRoot };
                results = new ADSearcher(searchRoot, filter, propertiesToLoad, SearchScope.OneLevel).FindAll();
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.DnsRoot);
                    DirectoryContext context = Utils.GetNewDirectoryContext(searchResultPropertyValue, DirectoryContextType.Domain, base.context);
                    list.Add(new Domain(context, searchResultPropertyValue));
                }
                return list;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
                if (searchRoot != null)
                {
                    searchRoot.Dispose();
                }
            }
            return list;
        }

        public static Domain GetComputerDomain()
        {
            string dnsDomainName = DirectoryContext.GetDnsDomainName(null);
            if (dnsDomainName == null)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ComputerNotJoinedToDomain"), typeof(Domain), null);
            }
            return GetDomain(new DirectoryContext(DirectoryContextType.Domain, dnsDomainName));
        }

        public static Domain GetCurrentDomain()
        {
            return GetDomain(new DirectoryContext(DirectoryContextType.Domain));
        }

        internal DirectoryContext GetDirectoryContext()
        {
            return base.context;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override DirectoryEntry GetDirectoryEntry()
        {
            base.CheckIfDisposed();
            return DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
        }

        public static Domain GetDomain(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.ContextType != DirectoryContextType.Domain) && (context.ContextType != DirectoryContextType.DirectoryServer))
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeServerORDomain"), "context");
            }
            if ((context.Name == null) && !context.isDomain())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(Domain), null);
            }
            if (((context.Name != null) && !context.isDomain()) && !context.isServer())
            {
                if (context.ContextType == DirectoryContextType.Domain)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainNotFound"), typeof(Domain), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(Domain), null);
            }
            context = new DirectoryContext(context);
            DirectoryEntryManager directoryEntryMgr = new DirectoryEntryManager(context);
            DirectoryEntry rootDSE = null;
            string distinguishedName = null;
            try
            {
                rootDSE = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (context.isServer() && !Utils.CheckCapability(rootDSE, Capability.ActiveDirectory))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(Domain), null);
                }
                distinguishedName = (string) PropertyManager.GetPropertyValue(context, rootDSE, PropertyManager.DefaultNamingContext);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016646)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if (context.ContextType == DirectoryContextType.Domain)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainNotFound"), typeof(Domain), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DCNotFound", new object[] { context.Name }), typeof(Domain), null);
            }
            return new Domain(context, Utils.GetDnsNameFromDN(distinguishedName), directoryEntryMgr);
        }

        private System.DirectoryServices.ActiveDirectory.DomainMode GetDomainMode()
        {
            System.DirectoryServices.ActiveDirectory.DomainMode mode;
            DirectoryEntry directoryEntry = null;
            DirectoryEntry entry2 = DirectoryEntryManager.GetDirectoryEntry(base.context, WellKnownDN.RootDSE);
            int num = 0;
            try
            {
                if (entry2.Properties.Contains(PropertyManager.DomainFunctionality))
                {
                    num = int.Parse((string) PropertyManager.GetPropertyValue(base.context, entry2, PropertyManager.DomainFunctionality), NumberFormatInfo.InvariantInfo);
                }
                switch (num)
                {
                    case 0:
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
                        if (((int) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.NTMixedDomain)) != 0)
                        {
                            return System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000MixedDomain;
                        }
                        return System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000NativeDomain;

                    case 1:
                        return System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003InterimDomain;

                    case 2:
                        return System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003Domain;

                    case 3:
                        return System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008Domain;

                    case 4:
                        return System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008R2Domain;
                }
                throw new ActiveDirectoryOperationException(Res.GetString("InvalidMode"));
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                entry2.Dispose();
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            return mode;
        }

        private Domain GetParent()
        {
            if (this.crossRefDN == null)
            {
                this.LoadCrossRefAttributes();
            }
            if (this.trustParent == null)
            {
                return null;
            }
            DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, this.trustParent);
            string name = null;
            DirectoryContext context = null;
            try
            {
                name = (string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.DnsRoot);
                context = Utils.GetNewDirectoryContext(name, DirectoryContextType.Domain, base.context);
            }
            finally
            {
                directoryEntry.Dispose();
            }
            return new Domain(context, name);
        }

        private DomainController GetRoleOwner(ActiveDirectoryRole role)
        {
            DirectoryEntry directoryEntry = null;
            string domainControllerName = null;
            try
            {
                switch (role)
                {
                    case ActiveDirectoryRole.PdcRole:
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
                        break;

                    case ActiveDirectoryRole.RidRole:
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RidManager));
                        break;

                    case ActiveDirectoryRole.InfrastructureRole:
                        directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.Infrastructure));
                        break;
                }
                domainControllerName = Utils.GetDnsHostNameFromNTDSA(base.context, (string) PropertyManager.GetPropertyValue(base.context, directoryEntry, PropertyManager.FsmoRoleOwner));
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            return new DomainController(Utils.GetNewDirectoryContext(domainControllerName, DirectoryContextType.DirectoryServer, base.context), domainControllerName);
        }

        public bool GetSelectiveAuthenticationStatus(string targetDomainName)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            return TrustHelper.GetTrustedDomainInfoStatus(base.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, false);
        }

        public bool GetSidFilteringStatus(string targetDomainName)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            return TrustHelper.GetTrustedDomainInfoStatus(base.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN, false);
        }

        public TrustRelationshipInformation GetTrustRelationship(string targetDomainName)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            ArrayList trustsHelper = this.GetTrustsHelper(targetDomainName);
            TrustRelationshipInformationCollection informations = new TrustRelationshipInformationCollection(base.context, base.Name, trustsHelper);
            if (informations.Count == 0)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DomainTrustDoesNotExist", new object[] { base.Name, targetDomainName }), typeof(TrustRelationshipInformation), null);
            }
            return informations[0];
        }

        private ArrayList GetTrustsHelper(string targetDomainName)
        {
            string serverName = null;
            ArrayList list3;
            IntPtr zero = IntPtr.Zero;
            int count = 0;
            ArrayList list = new ArrayList();
            ArrayList list2 = new ArrayList();
            new TrustRelationshipInformationCollection();
            int num2 = 0;
            string str2 = null;
            int errorCode = 0;
            bool flag = false;
            if (base.context.isServer())
            {
                serverName = base.context.Name;
            }
            else
            {
                serverName = DomainController.FindOne(base.context).Name;
            }
            flag = Utils.Impersonate(base.context);
            try
            {
                try
                {
                    errorCode = System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.DsEnumerateDomainTrustsW(serverName, 0x23, out zero, out count);
                }
                finally
                {
                    if (flag)
                    {
                        Utils.Revert();
                    }
                }
            }
            catch
            {
                throw;
            }
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode, serverName);
            }
            try
            {
                if ((zero != IntPtr.Zero) && (count != 0))
                {
                    IntPtr ptr = IntPtr.Zero;
                    int num4 = 0;
                    for (int i = 0; i < count; i++)
                    {
                        ptr = (IntPtr) (((long) zero) + (i * Marshal.SizeOf(typeof(DS_DOMAIN_TRUSTS))));
                        DS_DOMAIN_TRUSTS structure = new DS_DOMAIN_TRUSTS();
                        Marshal.PtrToStructure(ptr, structure);
                        list.Add(structure);
                    }
                    for (int j = 0; j < list.Count; j++)
                    {
                        DS_DOMAIN_TRUSTS ds_domain_trusts2 = (DS_DOMAIN_TRUSTS) list[j];
                        if (((ds_domain_trusts2.Flags & 0x2a) != 0) && (ds_domain_trusts2.TrustType != TrustHelper.TRUST_TYPE_DOWNLEVEL))
                        {
                            TrustObject obj2 = new TrustObject {
                                TrustType = TrustType.Unknown
                            };
                            if (ds_domain_trusts2.DnsDomainName != IntPtr.Zero)
                            {
                                obj2.DnsDomainName = Marshal.PtrToStringUni(ds_domain_trusts2.DnsDomainName);
                            }
                            if (ds_domain_trusts2.NetbiosDomainName != IntPtr.Zero)
                            {
                                obj2.NetbiosDomainName = Marshal.PtrToStringUni(ds_domain_trusts2.NetbiosDomainName);
                            }
                            obj2.Flags = ds_domain_trusts2.Flags;
                            obj2.TrustAttributes = ds_domain_trusts2.TrustAttributes;
                            obj2.OriginalIndex = j;
                            obj2.ParentIndex = ds_domain_trusts2.ParentIndex;
                            if (targetDomainName != null)
                            {
                                bool flag2 = false;
                                if ((obj2.DnsDomainName != null) && (Utils.Compare(targetDomainName, obj2.DnsDomainName) == 0))
                                {
                                    flag2 = true;
                                }
                                else if ((obj2.NetbiosDomainName != null) && (Utils.Compare(targetDomainName, obj2.NetbiosDomainName) == 0))
                                {
                                    flag2 = true;
                                }
                                if (!flag2 && ((obj2.Flags & 8) == 0))
                                {
                                    continue;
                                }
                            }
                            if ((obj2.Flags & 8) != 0)
                            {
                                num2 = num4;
                                if ((obj2.Flags & 4) == 0)
                                {
                                    DS_DOMAIN_TRUSTS ds_domain_trusts3 = (DS_DOMAIN_TRUSTS) list[obj2.ParentIndex];
                                    if (ds_domain_trusts3.DnsDomainName != IntPtr.Zero)
                                    {
                                        str2 = Marshal.PtrToStringUni(ds_domain_trusts3.DnsDomainName);
                                    }
                                }
                                obj2.TrustType = TrustType.Unknown | TrustType.ParentChild;
                            }
                            else if (ds_domain_trusts2.TrustType == 3)
                            {
                                obj2.TrustType = TrustType.Kerberos;
                            }
                            num4++;
                            list2.Add(obj2);
                        }
                    }
                    for (int k = 0; k < list2.Count; k++)
                    {
                        TrustObject obj3 = (TrustObject) list2[k];
                        if ((k != num2) && (obj3.TrustType != TrustType.Kerberos))
                        {
                            if ((str2 != null) && (Utils.Compare(str2, obj3.DnsDomainName) == 0))
                            {
                                obj3.TrustType = TrustType.ParentChild;
                            }
                            else if ((obj3.Flags & 1) != 0)
                            {
                                if (obj3.ParentIndex == ((TrustObject) list2[num2]).OriginalIndex)
                                {
                                    obj3.TrustType = TrustType.ParentChild;
                                }
                                else if (((obj3.Flags & 4) != 0) && ((((TrustObject) list2[num2]).Flags & 4) != 0))
                                {
                                    string dnsNameFromDN = null;
                                    dnsNameFromDN = Utils.GetDnsNameFromDN(base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.RootDomainNamingContext));
                                    if (Utils.GetNewDirectoryContext(base.context.Name, DirectoryContextType.Forest, base.context).isRootDomain() || (Utils.Compare(obj3.DnsDomainName, dnsNameFromDN) == 0))
                                    {
                                        obj3.TrustType = TrustType.TreeRoot;
                                    }
                                    else
                                    {
                                        obj3.TrustType = TrustType.CrossLink;
                                    }
                                }
                                else
                                {
                                    obj3.TrustType = TrustType.CrossLink;
                                }
                            }
                            else if ((obj3.TrustAttributes & 8) != 0)
                            {
                                obj3.TrustType = TrustType.Forest;
                            }
                            else
                            {
                                obj3.TrustType = TrustType.External;
                            }
                        }
                    }
                }
                list3 = list2;
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    System.DirectoryServices.ActiveDirectory.UnsafeNativeMethods.NetApiBufferFree(zero);
                }
            }
            return list3;
        }

        private void LoadCrossRefAttributes()
        {
            DirectoryEntry searchRoot = null;
            try
            {
                searchRoot = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.PartitionsContainer));
                StringBuilder builder = new StringBuilder(15);
                builder.Append("(&(");
                builder.Append(PropertyManager.ObjectCategory);
                builder.Append("=crossRef)(");
                builder.Append(PropertyManager.SystemFlags);
                builder.Append(":1.2.840.113556.1.4.804:=");
                builder.Append(1);
                builder.Append(")(");
                builder.Append(PropertyManager.SystemFlags);
                builder.Append(":1.2.840.113556.1.4.804:=");
                builder.Append(2);
                builder.Append(")(");
                builder.Append(PropertyManager.DnsRoot);
                builder.Append("=");
                builder.Append(Utils.GetEscapedFilterValue(base.partitionName));
                builder.Append("))");
                string filter = builder.ToString();
                string[] propertiesToLoad = new string[] { PropertyManager.DistinguishedName, PropertyManager.TrustParent };
                SearchResult res = new ADSearcher(searchRoot, filter, propertiesToLoad, SearchScope.OneLevel, false, false).FindOne();
                this.crossRefDN = (string) PropertyManager.GetSearchResultPropertyValue(res, PropertyManager.DistinguishedName);
                if (res.Properties[PropertyManager.TrustParent].Count > 0)
                {
                    this.trustParent = (string) res.Properties[PropertyManager.TrustParent][0];
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                if (searchRoot != null)
                {
                    searchRoot.Dispose();
                }
            }
        }

        public void RaiseDomainFunctionality(System.DirectoryServices.ActiveDirectory.DomainMode domainMode)
        {
            base.CheckIfDisposed();
            if ((domainMode < System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000MixedDomain) || (domainMode > System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008R2Domain))
            {
                throw new InvalidEnumArgumentException("domainMode", (int) domainMode, typeof(System.DirectoryServices.ActiveDirectory.DomainMode));
            }
            System.DirectoryServices.ActiveDirectory.DomainMode mode = this.GetDomainMode();
            DirectoryEntry directoryEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));
                switch (mode)
                {
                    case System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000MixedDomain:
                        if (domainMode != System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000NativeDomain)
                        {
                            break;
                        }
                        directoryEntry.Properties[PropertyManager.NTMixedDomain].Value = 0;
                        goto Label_0286;

                    case System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000NativeDomain:
                        if (domainMode != System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003Domain)
                        {
                            goto Label_012A;
                        }
                        directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 2;
                        goto Label_0286;

                    case System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003InterimDomain:
                        if (domainMode != System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003Domain)
                        {
                            throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
                        }
                        directoryEntry.Properties[PropertyManager.NTMixedDomain].Value = 0;
                        directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 2;
                        goto Label_0286;

                    case System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003Domain:
                        if (domainMode != System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008Domain)
                        {
                            goto Label_01FF;
                        }
                        directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 3;
                        goto Label_0286;

                    case System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008Domain:
                        if (domainMode != System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008R2Domain)
                        {
                            throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
                        }
                        directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 4;
                        goto Label_0286;

                    case System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008R2Domain:
                        throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");

                    default:
                        throw new ActiveDirectoryOperationException();
                }
                if (domainMode == System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003InterimDomain)
                {
                    directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 1;
                    goto Label_0286;
                }
                if (domainMode == System.DirectoryServices.ActiveDirectory.DomainMode.Windows2003Domain)
                {
                    directoryEntry.Properties[PropertyManager.NTMixedDomain].Value = 0;
                    directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 2;
                    goto Label_0286;
                }
                throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
            Label_012A:
                if (domainMode == System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008Domain)
                {
                    directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 3;
                    goto Label_0286;
                }
                if (domainMode == System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008R2Domain)
                {
                    directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 4;
                    goto Label_0286;
                }
                throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
            Label_01FF:
                if (domainMode == System.DirectoryServices.ActiveDirectory.DomainMode.Windows2008R2Domain)
                {
                    directoryEntry.Properties[PropertyManager.MsDSBehaviorVersion].Value = 4;
                }
                else
                {
                    throw new ArgumentException(Res.GetString("InvalidMode"), "domainMode");
                }
            Label_0286:
                directoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016694)
                {
                    throw new ArgumentException(Res.GetString("NoW2K3DCs"), "domainMode");
                }
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            this.currentDomainMode = ~System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000MixedDomain;
        }

        private void RepairTrustHelper(Domain targetDomain, TrustDirection direction)
        {
            string password = TrustHelper.CreateTrustPassword();
            string preferredTargetServer = TrustHelper.UpdateTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, password, false);
            string str3 = TrustHelper.UpdateTrust(base.context, base.Name, targetDomain.Name, password, false);
            if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(base.context, base.Name, targetDomain.Name, false, TrustDirection.Outbound, true, preferredTargetServer);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { base.Name, targetDomain.Name, direction }), typeof(TrustRelationshipInformation), null);
                }
            }
            if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false, TrustDirection.Outbound, true, str3);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { base.Name, targetDomain.Name, direction }), typeof(TrustRelationshipInformation), null);
                }
            }
        }

        public void RepairTrustRelationship(Domain targetDomain)
        {
            TrustDirection bidirectional = TrustDirection.Bidirectional;
            base.CheckIfDisposed();
            if (targetDomain == null)
            {
                throw new ArgumentNullException("targetDomain");
            }
            try
            {
                bidirectional = this.GetTrustRelationship(targetDomain.Name).TrustDirection;
                if ((bidirectional & TrustDirection.Outbound) != ((TrustDirection) 0))
                {
                    TrustHelper.VerifyTrust(base.context, base.Name, targetDomain.Name, false, TrustDirection.Outbound, true, null);
                }
                if ((bidirectional & TrustDirection.Inbound) != ((TrustDirection) 0))
                {
                    TrustHelper.VerifyTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false, TrustDirection.Outbound, true, null);
                }
            }
            catch (ActiveDirectoryOperationException)
            {
                this.RepairTrustHelper(targetDomain, bidirectional);
            }
            catch (UnauthorizedAccessException)
            {
                this.RepairTrustHelper(targetDomain, bidirectional);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { base.Name, targetDomain.Name, bidirectional }), typeof(TrustRelationshipInformation), null);
            }
        }

        public void SetSelectiveAuthenticationStatus(string targetDomainName, bool enable)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            TrustHelper.SetTrustedDomainInfoStatus(base.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_CROSS_ORGANIZATION, enable, false);
        }

        public void SetSidFilteringStatus(string targetDomainName, bool enable)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            TrustHelper.SetTrustedDomainInfoStatus(base.context, base.Name, targetDomainName, TRUST_ATTRIBUTE.TRUST_ATTRIBUTE_QUARANTINED_DOMAIN, enable, false);
        }

        public void UpdateLocalSideOfTrustRelationship(string targetDomainName, string newTrustPassword)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            if (newTrustPassword == null)
            {
                throw new ArgumentNullException("newTrustPassword");
            }
            if (newTrustPassword.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "newTrustPassword");
            }
            TrustHelper.UpdateTrust(base.context, base.Name, targetDomainName, newTrustPassword, false);
        }

        public void UpdateLocalSideOfTrustRelationship(string targetDomainName, TrustDirection newTrustDirection, string newTrustPassword)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            if ((newTrustDirection < TrustDirection.Inbound) || (newTrustDirection > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("newTrustDirection", (int) newTrustDirection, typeof(TrustDirection));
            }
            if (newTrustPassword == null)
            {
                throw new ArgumentNullException("newTrustPassword");
            }
            if (newTrustPassword.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "newTrustPassword");
            }
            TrustHelper.UpdateTrustDirection(base.context, base.Name, targetDomainName, newTrustPassword, false, newTrustDirection);
        }

        public void UpdateTrustRelationship(Domain targetDomain, TrustDirection newTrustDirection)
        {
            base.CheckIfDisposed();
            if (targetDomain == null)
            {
                throw new ArgumentNullException("targetDomain");
            }
            if ((newTrustDirection < TrustDirection.Inbound) || (newTrustDirection > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("newTrustDirection", (int) newTrustDirection, typeof(TrustDirection));
            }
            string password = TrustHelper.CreateTrustPassword();
            TrustHelper.UpdateTrustDirection(base.context, base.Name, targetDomain.Name, password, false, newTrustDirection);
            TrustDirection direction = (TrustDirection) 0;
            if ((newTrustDirection & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                direction |= TrustDirection.Outbound;
            }
            if ((newTrustDirection & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                direction |= TrustDirection.Inbound;
            }
            TrustHelper.UpdateTrustDirection(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, password, false, direction);
        }

        public void VerifyOutboundTrustRelationship(string targetDomainName)
        {
            base.CheckIfDisposed();
            if (targetDomainName == null)
            {
                throw new ArgumentNullException("targetDomainName");
            }
            if (targetDomainName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "targetDomainName");
            }
            TrustHelper.VerifyTrust(base.context, base.Name, targetDomainName, false, TrustDirection.Outbound, false, null);
        }

        public void VerifyTrustRelationship(Domain targetDomain, TrustDirection direction)
        {
            base.CheckIfDisposed();
            if (targetDomain == null)
            {
                throw new ArgumentNullException("targetDomain");
            }
            if ((direction < TrustDirection.Inbound) || (direction > TrustDirection.Bidirectional))
            {
                throw new InvalidEnumArgumentException("direction", (int) direction, typeof(TrustDirection));
            }
            if ((direction & TrustDirection.Outbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(base.context, base.Name, targetDomain.Name, false, TrustDirection.Outbound, false, null);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { base.Name, targetDomain.Name, direction }), typeof(TrustRelationshipInformation), null);
                }
            }
            if ((direction & TrustDirection.Inbound) != ((TrustDirection) 0))
            {
                try
                {
                    TrustHelper.VerifyTrust(targetDomain.GetDirectoryContext(), targetDomain.Name, base.Name, false, TrustDirection.Outbound, false, null);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("WrongTrustDirection", new object[] { base.Name, targetDomain.Name, direction }), typeof(TrustRelationshipInformation), null);
                }
            }
        }

        public DomainCollection Children
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedChildren == null)
                {
                    this.cachedChildren = new DomainCollection(this.GetChildDomains());
                }
                return this.cachedChildren;
            }
        }

        public DomainControllerCollection DomainControllers
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedDomainControllers == null)
                {
                    this.cachedDomainControllers = this.FindAllDomainControllers();
                }
                return this.cachedDomainControllers;
            }
        }

        public System.DirectoryServices.ActiveDirectory.DomainMode DomainMode
        {
            get
            {
                base.CheckIfDisposed();
                if (this.currentDomainMode == ~System.DirectoryServices.ActiveDirectory.DomainMode.Windows2000MixedDomain)
                {
                    this.currentDomainMode = this.GetDomainMode();
                }
                return this.currentDomainMode;
            }
        }

        public System.DirectoryServices.ActiveDirectory.Forest Forest
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedForest == null)
                {
                    DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                    string distinguishedName = (string) PropertyManager.GetPropertyValue(base.context, cachedDirectoryEntry, PropertyManager.RootDomainNamingContext);
                    string dnsNameFromDN = Utils.GetDnsNameFromDN(distinguishedName);
                    DirectoryContext context = Utils.GetNewDirectoryContext(dnsNameFromDN, DirectoryContextType.Forest, base.context);
                    this.cachedForest = new System.DirectoryServices.ActiveDirectory.Forest(context, dnsNameFromDN);
                }
                return this.cachedForest;
            }
        }

        public DomainController InfrastructureRoleOwner
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedInfrastructureRoleOwner == null)
                {
                    this.cachedInfrastructureRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.InfrastructureRole);
                }
                return this.cachedInfrastructureRoleOwner;
            }
        }

        public Domain Parent
        {
            get
            {
                base.CheckIfDisposed();
                if (!this.isParentInitialized)
                {
                    this.cachedParent = this.GetParent();
                    this.isParentInitialized = true;
                }
                return this.cachedParent;
            }
        }

        public DomainController PdcRoleOwner
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedPdcRoleOwner == null)
                {
                    this.cachedPdcRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.PdcRole);
                }
                return this.cachedPdcRoleOwner;
            }
        }

        public DomainController RidRoleOwner
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedRidRoleOwner == null)
                {
                    this.cachedRidRoleOwner = this.GetRoleOwner(ActiveDirectoryRole.RidRole);
                }
                return this.cachedRidRoleOwner;
            }
        }
    }
}

