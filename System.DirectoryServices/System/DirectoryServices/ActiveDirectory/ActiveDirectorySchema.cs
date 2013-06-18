namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectorySchema : ActiveDirectoryPartition
    {
        private DirectoryEntry abstractSchemaEntry;
        private DirectoryServer cachedSchemaRoleOwner;
        private bool disposed;
        private DirectoryEntry schemaEntry;

        internal ActiveDirectorySchema(DirectoryContext context, string distinguishedName) : base(context, distinguishedName)
        {
            base.directoryEntryMgr = new DirectoryEntryManager(context);
            this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(context, distinguishedName);
        }

        internal ActiveDirectorySchema(DirectoryContext context, string distinguishedName, DirectoryEntryManager directoryEntryMgr) : base(context, distinguishedName)
        {
            base.directoryEntryMgr = directoryEntryMgr;
            this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(context, distinguishedName);
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                try
                {
                    if (disposing)
                    {
                        if (this.schemaEntry != null)
                        {
                            this.schemaEntry.Dispose();
                            this.schemaEntry = null;
                        }
                        if (this.abstractSchemaEntry != null)
                        {
                            this.abstractSchemaEntry.Dispose();
                            this.abstractSchemaEntry = null;
                        }
                    }
                    this.disposed = true;
                }
                finally
                {
                    base.Dispose();
                }
            }
        }

        public ReadOnlyActiveDirectorySchemaClassCollection FindAllClasses()
        {
            base.CheckIfDisposed();
            string filter = "(&(" + PropertyManager.ObjectCategory + "=classSchema)(!(" + PropertyManager.IsDefunct + "=TRUE)))";
            return GetAllClasses(base.context, this.schemaEntry, filter);
        }

        public ReadOnlyActiveDirectorySchemaClassCollection FindAllClasses(SchemaClassType type)
        {
            base.CheckIfDisposed();
            if ((type < SchemaClassType.Type88) || (type > SchemaClassType.Auxiliary))
            {
                throw new InvalidEnumArgumentException("type", (int) type, typeof(SchemaClassType));
            }
            string filter = string.Concat(new object[] { "(&(", PropertyManager.ObjectCategory, "=classSchema)(", PropertyManager.ObjectClassCategory, "=", (int) type, ")(!(", PropertyManager.IsDefunct, "=TRUE)))" });
            return GetAllClasses(base.context, this.schemaEntry, filter);
        }

        public ReadOnlyActiveDirectorySchemaClassCollection FindAllDefunctClasses()
        {
            base.CheckIfDisposed();
            string filter = "(&(" + PropertyManager.ObjectCategory + "=classSchema)(" + PropertyManager.IsDefunct + "=TRUE))";
            return GetAllClasses(base.context, this.schemaEntry, filter);
        }

        public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllDefunctProperties()
        {
            base.CheckIfDisposed();
            string filter = "(&(" + PropertyManager.ObjectCategory + "=attributeSchema)(" + PropertyManager.IsDefunct + "=TRUE))";
            return GetAllProperties(base.context, this.schemaEntry, filter);
        }

        public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties()
        {
            base.CheckIfDisposed();
            string filter = "(&(" + PropertyManager.ObjectCategory + "=attributeSchema)(!(" + PropertyManager.IsDefunct + "=TRUE)))";
            return GetAllProperties(base.context, this.schemaEntry, filter);
        }

        public ReadOnlyActiveDirectorySchemaPropertyCollection FindAllProperties(PropertyTypes type)
        {
            base.CheckIfDisposed();
            if ((type & ~(PropertyTypes.InGlobalCatalog | PropertyTypes.Indexed)) != 0)
            {
                throw new ArgumentException(Res.GetString("InvalidFlags"), "type");
            }
            StringBuilder builder = new StringBuilder(0x19);
            builder.Append("(&(");
            builder.Append(PropertyManager.ObjectCategory);
            builder.Append("=attributeSchema)");
            builder.Append("(!(");
            builder.Append(PropertyManager.IsDefunct);
            builder.Append("=TRUE))");
            if ((type & PropertyTypes.Indexed) != 0)
            {
                builder.Append("(");
                builder.Append(PropertyManager.SearchFlags);
                builder.Append(":1.2.840.113556.1.4.804:=");
                builder.Append(1);
                builder.Append(")");
            }
            if ((type & PropertyTypes.InGlobalCatalog) != 0)
            {
                builder.Append("(");
                builder.Append(PropertyManager.IsMemberOfPartialAttributeSet);
                builder.Append("=TRUE)");
            }
            builder.Append(")");
            return GetAllProperties(base.context, this.schemaEntry, builder.ToString());
        }

        public ActiveDirectorySchemaClass FindClass(string ldapDisplayName)
        {
            base.CheckIfDisposed();
            return ActiveDirectorySchemaClass.FindByName(base.context, ldapDisplayName);
        }

        public ActiveDirectorySchemaClass FindDefunctClass(string commonName)
        {
            base.CheckIfDisposed();
            if (commonName == null)
            {
                throw new ArgumentNullException("commonName");
            }
            if (commonName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "commonName");
            }
            return new ActiveDirectorySchemaClass(base.context, commonName, ActiveDirectorySchemaClass.GetPropertiesFromSchemaContainer(base.context, this.schemaEntry, commonName, true), this.schemaEntry);
        }

        public ActiveDirectorySchemaProperty FindDefunctProperty(string commonName)
        {
            base.CheckIfDisposed();
            if (commonName == null)
            {
                throw new ArgumentNullException("commonName");
            }
            if (commonName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "commonName");
            }
            return new ActiveDirectorySchemaProperty(base.context, commonName, ActiveDirectorySchemaProperty.GetPropertiesFromSchemaContainer(base.context, this.schemaEntry, commonName, true), this.schemaEntry);
        }

        public ActiveDirectorySchemaProperty FindProperty(string ldapDisplayName)
        {
            base.CheckIfDisposed();
            return ActiveDirectorySchemaProperty.FindByName(base.context, ldapDisplayName);
        }

        internal static ReadOnlyActiveDirectorySchemaClassCollection GetAllClasses(DirectoryContext context, DirectoryEntry schemaEntry, string filter)
        {
            ArrayList values = new ArrayList();
            string[] propertiesToLoad = new string[] { PropertyManager.LdapDisplayName, PropertyManager.Cn, PropertyManager.IsDefunct };
            ADSearcher searcher = new ADSearcher(schemaEntry, filter, propertiesToLoad, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.LdapDisplayName);
                    DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                    directoryEntry.AuthenticationType = Utils.DefaultAuthType;
                    directoryEntry.Username = context.UserName;
                    directoryEntry.Password = context.Password;
                    bool flag = false;
                    if ((result.Properties[PropertyManager.IsDefunct] != null) && (result.Properties[PropertyManager.IsDefunct].Count > 0))
                    {
                        flag = (bool) result.Properties[PropertyManager.IsDefunct][0];
                    }
                    if (flag)
                    {
                        string commonName = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                        values.Add(new ActiveDirectorySchemaClass(context, commonName, searchResultPropertyValue, directoryEntry, schemaEntry));
                    }
                    else
                    {
                        values.Add(new ActiveDirectorySchemaClass(context, searchResultPropertyValue, directoryEntry, schemaEntry));
                    }
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
            }
            return new ReadOnlyActiveDirectorySchemaClassCollection(values);
        }

        internal static ReadOnlyActiveDirectorySchemaPropertyCollection GetAllProperties(DirectoryContext context, DirectoryEntry schemaEntry, string filter)
        {
            ArrayList values = new ArrayList();
            string[] propertiesToLoad = new string[] { PropertyManager.LdapDisplayName, PropertyManager.Cn, PropertyManager.IsDefunct };
            ADSearcher searcher = new ADSearcher(schemaEntry, filter, propertiesToLoad, SearchScope.OneLevel);
            SearchResultCollection results = null;
            try
            {
                results = searcher.FindAll();
                foreach (SearchResult result in results)
                {
                    string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.LdapDisplayName);
                    DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                    directoryEntry.AuthenticationType = Utils.DefaultAuthType;
                    directoryEntry.Username = context.UserName;
                    directoryEntry.Password = context.Password;
                    bool flag = false;
                    if ((result.Properties[PropertyManager.IsDefunct] != null) && (result.Properties[PropertyManager.IsDefunct].Count > 0))
                    {
                        flag = (bool) result.Properties[PropertyManager.IsDefunct][0];
                    }
                    if (flag)
                    {
                        string commonName = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.Cn);
                        values.Add(new ActiveDirectorySchemaProperty(context, commonName, searchResultPropertyValue, directoryEntry, schemaEntry));
                    }
                    else
                    {
                        values.Add(new ActiveDirectorySchemaProperty(context, searchResultPropertyValue, directoryEntry, schemaEntry));
                    }
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
            }
            return new ReadOnlyActiveDirectorySchemaPropertyCollection(values);
        }

        public static ActiveDirectorySchema GetCurrentSchema()
        {
            return GetSchema(new DirectoryContext(DirectoryContextType.Forest));
        }

        [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true), DirectoryServicesPermission(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public override DirectoryEntry GetDirectoryEntry()
        {
            base.CheckIfDisposed();
            return DirectoryEntryManager.GetDirectoryEntry(base.context, base.Name);
        }

        public static ActiveDirectorySchema GetSchema(DirectoryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (((context.ContextType != DirectoryContextType.Forest) && (context.ContextType != DirectoryContextType.ConfigurationSet)) && (context.ContextType != DirectoryContextType.DirectoryServer))
            {
                throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ContextNotAssociatedWithDomain"), typeof(ActiveDirectorySchema), null);
            }
            if (((context.Name != null) && !context.isRootDomain()) && (!context.isADAMConfigSet() && !context.isServer()))
            {
                if (context.ContextType == DirectoryContextType.Forest)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(ActiveDirectorySchema), context.Name);
                }
                if (context.ContextType == DirectoryContextType.ConfigurationSet)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ActiveDirectorySchema), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ServerNotFound", new object[] { context.Name }), typeof(ActiveDirectorySchema), null);
            }
            context = new DirectoryContext(context);
            DirectoryEntryManager directoryEntryMgr = new DirectoryEntryManager(context);
            string distinguishedName = null;
            try
            {
                DirectoryEntry cachedDirectoryEntry = directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE);
                if (context.isServer() && !Utils.CheckCapability(cachedDirectoryEntry, Capability.ActiveDirectoryOrADAM))
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ServerNotFound", new object[] { context.Name }), typeof(ActiveDirectorySchema), null);
                }
                distinguishedName = (string) PropertyManager.GetPropertyValue(context, cachedDirectoryEntry, PropertyManager.SchemaNamingContext);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147016646)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
                }
                if (context.ContextType == DirectoryContextType.Forest)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ForestNotFound"), typeof(ActiveDirectorySchema), context.Name);
                }
                if (context.ContextType == DirectoryContextType.ConfigurationSet)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ActiveDirectorySchema), context.Name);
                }
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ServerNotFound", new object[] { context.Name }), typeof(ActiveDirectorySchema), null);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                if (context.ContextType == DirectoryContextType.ConfigurationSet)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("ConfigSetNotFound"), typeof(ActiveDirectorySchema), context.Name);
                }
                throw;
            }
            return new ActiveDirectorySchema(context, distinguishedName, directoryEntryMgr);
        }

        private DirectoryServer GetSchemaRoleOwner()
        {
            DirectoryServer server2;
            try
            {
                this.schemaEntry.RefreshCache();
                if (base.context.isADAMConfigSet())
                {
                    string adamDnsHostNameFromNTDSA = Utils.GetAdamDnsHostNameFromNTDSA(base.context, (string) PropertyManager.GetPropertyValue(base.context, this.schemaEntry, PropertyManager.FsmoRoleOwner));
                    return new AdamInstance(Utils.GetNewDirectoryContext(adamDnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, base.context), adamDnsHostNameFromNTDSA);
                }
                DirectoryServer server = null;
                if (Utils.CheckCapability(base.directoryEntryMgr.GetCachedDirectoryEntry(WellKnownDN.RootDSE), Capability.ActiveDirectory))
                {
                    string dnsHostNameFromNTDSA = Utils.GetDnsHostNameFromNTDSA(base.context, (string) PropertyManager.GetPropertyValue(base.context, this.schemaEntry, PropertyManager.FsmoRoleOwner));
                    server = new DomainController(Utils.GetNewDirectoryContext(dnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, base.context), dnsHostNameFromNTDSA);
                }
                else
                {
                    string adamInstanceName = Utils.GetAdamDnsHostNameFromNTDSA(base.context, (string) PropertyManager.GetPropertyValue(base.context, this.schemaEntry, PropertyManager.FsmoRoleOwner));
                    server = new AdamInstance(Utils.GetNewDirectoryContext(adamInstanceName, DirectoryContextType.DirectoryServer, base.context), adamInstanceName);
                }
                server2 = server;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(base.context, exception);
            }
            return server2;
        }

        public void RefreshSchema()
        {
            base.CheckIfDisposed();
            DirectoryEntry directoryEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(base.context, WellKnownDN.RootDSE);
                directoryEntry.Properties[PropertyManager.SchemaUpdateNow].Value = 1;
                directoryEntry.CommitChanges();
                if (this.abstractSchemaEntry == null)
                {
                    this.abstractSchemaEntry = base.directoryEntryMgr.GetCachedDirectoryEntry("Schema");
                }
                this.abstractSchemaEntry.RefreshCache();
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
        }

        public DirectoryServer SchemaRoleOwner
        {
            get
            {
                base.CheckIfDisposed();
                if (this.cachedSchemaRoleOwner == null)
                {
                    this.cachedSchemaRoleOwner = this.GetSchemaRoleOwner();
                }
                return this.cachedSchemaRoleOwner;
            }
        }
    }
}

