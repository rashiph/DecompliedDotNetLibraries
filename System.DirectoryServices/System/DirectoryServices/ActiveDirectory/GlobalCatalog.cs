namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class GlobalCatalog : DomainController
    {
        private bool disabled;
        private ActiveDirectorySchema schema;

        internal GlobalCatalog(DirectoryContext context, string globalCatalogName) : base(context, globalCatalogName)
        {
        }

        internal GlobalCatalog(DirectoryContext context, string globalCatalogName, DirectoryEntryManager directoryEntryMgr) : base(context, globalCatalogName, directoryEntryMgr)
        {
        }

        private void CheckIfDisabled()
        {
            if (this.disabled)
            {
                throw new InvalidOperationException(Res.GetString("GCDisabled"));
            }
        }

        public DomainController DisableGlobalCatalog()
        {
            base.CheckIfDisposed();
            this.CheckIfDisabled();
            DirectoryEntry cachedDirectoryEntry = base.directoryEntryMgr.GetCachedDirectoryEntry(base.NtdsaObjectName);
            int num = 0;
            try
            {
                if (cachedDirectoryEntry.Properties[PropertyManager.Options].Value != null)
                {
                    num = (int) cachedDirectoryEntry.Properties[PropertyManager.Options].Value;
                }
                cachedDirectoryEntry.Properties[PropertyManager.Options].Value = num & -2;
                cachedDirectoryEntry.CommitChanges();
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            this.disabled = true;
            return new DomainController(base.context, base.Name);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override GlobalCatalog EnableGlobalCatalog()
        {
            base.CheckIfDisposed();
            throw new InvalidOperationException(Res.GetString("CannotPerformOnGCObject"));
        }

        public static GlobalCatalogCollection FindAll(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Forest)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
            }
            context = new DirectoryContext(context);
            return FindAllInternal(context, null);
        }

        public static GlobalCatalogCollection FindAll(DirectoryContext context, string siteName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Forest)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            context = new DirectoryContext(context);
            return FindAllInternal(context, siteName);
        }

        internal static GlobalCatalogCollection FindAllInternal(DirectoryContext context, string siteName)
        {
            ArrayList values = new ArrayList();
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            foreach (string str in Utils.GetReplicaList(context, null, siteName, false, false, true))
            {
                DirectoryContext context2 = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
                values.Add(new GlobalCatalog(context2, str));
            }
            return new GlobalCatalogCollection(values);
        }

        public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties()
        {
            base.CheckIfDisposed();
            this.CheckIfDisabled();
            if (this.schema == null)
            {
                string distinguishedName = null;
                try
                {
                    distinguishedName = base.directoryEntryMgr.ExpandWellKnownDN(WellKnownDN.SchemaNamingContext);
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
                }
                Utils.GetNewDirectoryContext(base.Name, DirectoryContextType.DirectoryServer, base.context);
                this.schema = new ActiveDirectorySchema(base.context, distinguishedName);
            }
            return this.schema.FindAllProperties(PropertyTypes.InGlobalCatalog);
        }

        public static GlobalCatalog FindOne(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Forest)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
            }
            return FindOneWithCredentialValidation(context, null, 0L);
        }

        public static GlobalCatalog FindOne(DirectoryContext context, LocatorOptions flag)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Forest)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
            }
            return FindOneWithCredentialValidation(context, null, flag);
        }

        public static GlobalCatalog FindOne(DirectoryContext context, string siteName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Forest)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return FindOneWithCredentialValidation(context, siteName, 0L);
        }

        public static GlobalCatalog FindOne(DirectoryContext context, string siteName, LocatorOptions flag)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.Forest)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeForest"), "context");
            }
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            return FindOneWithCredentialValidation(context, siteName, flag);
        }

        internal static GlobalCatalog FindOneInternal(DirectoryContext context, string forestName, string siteName, LocatorOptions flag)
        {
            DomainControllerInfo info;
            int errorCode = 0;
            if ((siteName != null) && (siteName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            if ((flag & ~(LocatorOptions.AvoidSelf | LocatorOptions.ForceRediscovery | LocatorOptions.KdcRequired | LocatorOptions.TimeServerRequired | LocatorOptions.WriteableRequired)) != 0L)
            {
                throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
            }
            if (forestName == null)
            {
                DomainControllerInfo info2;
                int num2 = Locator.DsGetDcNameWrapper(null, DirectoryContext.GetLoggedOnDomain(), null, 0x10L, out info2);
                if (num2 == 0x54b)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(GlobalCatalog), null);
                }
                if (num2 != 0)
                {
                    throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
                }
                forestName = info2.DnsForestName;
            }
            errorCode = Locator.DsGetDcNameWrapper(null, forestName, siteName, flag | 80L, out info);
            switch (errorCode)
            {
                case 0x54b:
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFoundInForest", new object[] { forestName }), typeof(GlobalCatalog), null);

                case 0x3ec:
                    throw new ArgumentException(Res.GetString("InvalidFlags"), "flag");
            }
            if (errorCode != 0)
            {
                throw ExceptionHelper.GetExceptionFromErrorCode(errorCode);
            }
            string globalCatalogName = info.DomainControllerName.Substring(2);
            return new GlobalCatalog(Utils.GetNewDirectoryContext(globalCatalogName, DirectoryContextType.DirectoryServer, context), globalCatalogName);
        }

        internal static GlobalCatalog FindOneWithCredentialValidation(DirectoryContext context, string siteName, LocatorOptions flag)
        {
            bool flag2 = false;
            bool flag3 = false;
            context = new DirectoryContext(context);
            GlobalCatalog dc = FindOneInternal(context, context.Name, siteName, flag);
            try
            {
                DomainController.ValidateCredential(dc, context);
                flag3 = true;
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016646)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if ((flag & LocatorOptions.ForceRediscovery) != 0L)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFoundInForest", new object[] { context.Name }), typeof(GlobalCatalog), null);
                }
                flag2 = true;
            }
            finally
            {
                if (!flag3)
                {
                    dc.Dispose();
                }
            }
            if (flag2)
            {
                flag3 = false;
                dc = FindOneInternal(context, context.Name, siteName, flag | LocatorOptions.ForceRediscovery);
                try
                {
                    DomainController.ValidateCredential(dc, context);
                    flag3 = true;
                }
                catch (COMException exception2)
                {
                    if (exception2.ErrorCode == -2147016646)
                    {
                        throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFoundInForest", new object[] { context.Name }), typeof(GlobalCatalog), null);
                    }
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception2);
                }
                finally
                {
                    if (!flag3)
                    {
                        dc.Dispose();
                    }
                }
            }
            return dc;
        }

        [DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
        public override DirectorySearcher GetDirectorySearcher()
        {
            base.CheckIfDisposed();
            this.CheckIfDisabled();
            return this.InternalGetDirectorySearcher();
        }

        public static GlobalCatalog GetGlobalCatalog(DirectoryContext context)
        {
            string globalCatalogName = null;
            DirectoryEntryManager directoryEntryMgr = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (context.ContextType != DirectoryContextType.DirectoryServer)
            {
                throw new ArgumentException(Res.GetString("TargetShouldBeGC"), "context");
            }
            if (!context.isServer())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", new object[] { context.Name }), typeof(GlobalCatalog), context.Name);
            }
            context = new DirectoryContext(context);
            try
            {
                directoryEntryMgr = new DirectoryEntryManager(context);
                DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                if (!Utils.CheckCapability(directoryEntry, Capability.ActiveDirectory))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", new object[] { context.Name }), typeof(GlobalCatalog), context.Name);
                }
                globalCatalogName = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.DnsHostName);
                if (!bool.Parse((string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.IsGlobalCatalogReady)))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", new object[] { context.Name }), typeof(GlobalCatalog), context.Name);
                }
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016646)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("GCNotFound", new object[] { context.Name }), typeof(GlobalCatalog), context.Name);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            return new GlobalCatalog(context, globalCatalogName, directoryEntryMgr);
        }

        [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
        private DirectorySearcher InternalGetDirectorySearcher()
        {
            DirectoryEntry searchRoot = new DirectoryEntry("GC://" + base.Name);
            if (DirectoryContext.ServerBindSupported)
            {
                searchRoot.AuthenticationType = Utils.DefaultAuthType | AuthenticationTypes.ServerBind;
            }
            else
            {
                searchRoot.AuthenticationType = Utils.DefaultAuthType;
            }
            searchRoot.Username = base.context.UserName;
            searchRoot.Password = base.context.Password;
            return new DirectorySearcher(searchRoot);
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override bool IsGlobalCatalog()
        {
            base.CheckIfDisposed();
            this.CheckIfDisabled();
            return true;
        }
    }
}

