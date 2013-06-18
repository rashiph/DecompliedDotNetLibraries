namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectorySubnet : IDisposable
    {
        internal DirectoryEntry cachedEntry;
        internal DirectoryContext context;
        private bool disposed;
        internal bool existing;
        private string name;
        private ActiveDirectorySite site;

        public ActiveDirectorySubnet(DirectoryContext context, string subnetName)
        {
            ValidateArgument(context, subnetName);
            context = new DirectoryContext(context);
            this.context = context;
            this.name = subnetName;
            DirectoryEntry directoryEntry = null;
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
                string dn = "CN=Subnets,CN=Sites," + str;
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dn);
                string escapedPath = Utils.GetEscapedPath("cn=" + this.name);
                this.cachedEntry = directoryEntry.Children.Add(escapedPath, "subnet");
            }
            catch (COMException exception)
            {
                ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
        }

        public ActiveDirectorySubnet(DirectoryContext context, string subnetName, string siteName) : this(context, subnetName)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName");
            }
            if (siteName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "siteName");
            }
            try
            {
                this.site = ActiveDirectorySite.FindByName(this.context, siteName);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ArgumentException(Res.GetString("SiteNotExist", new object[] { siteName }), "siteName");
            }
        }

        internal ActiveDirectorySubnet(DirectoryContext context, string subnetName, string siteName, bool existing)
        {
            this.context = context;
            this.name = subnetName;
            if (siteName != null)
            {
                try
                {
                    this.site = ActiveDirectorySite.FindByName(context, siteName);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ArgumentException(Res.GetString("SiteNotExist", new object[] { siteName }), "siteName");
                }
            }
            this.existing = true;
        }

        public void Delete()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.existing)
            {
                throw new InvalidOperationException(Res.GetString("CannotDelete"));
            }
            try
            {
                this.cachedEntry.Parent.Children.Remove(this.cachedEntry);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.cachedEntry != null))
            {
                this.cachedEntry.Dispose();
            }
            this.disposed = true;
        }

        public static ActiveDirectorySubnet FindByName(DirectoryContext context, string subnetName)
        {
            DirectoryEntry directoryEntry;
            ActiveDirectorySubnet subnet2;
            ValidateArgument(context, subnetName);
            context = new DirectoryContext(context);
            try
            {
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
                string str = (string) PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.ConfigurationNamingContext);
                string dn = "CN=Subnets,CN=Sites," + str;
                directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, dn);
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
            try
            {
                SearchResult result = new ADSearcher(directoryEntry, "(&(objectClass=subnet)(objectCategory=subnet)(name=" + Utils.GetEscapedFilterValue(subnetName) + "))", new string[] { "distinguishedName" }, SearchScope.OneLevel, false, false).FindOne();
                if (result == null)
                {
                    Exception exception2 = new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySubnet), subnetName);
                    throw exception2;
                }
                string siteName = null;
                DirectoryEntry entry2 = result.GetDirectoryEntry();
                if (entry2.Properties.Contains("siteObject"))
                {
                    NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
                    pathname.EscapedMode = 4;
                    string bstrADsPath = (string) entry2.Properties["siteObject"][0];
                    pathname.Set(bstrADsPath, 4);
                    siteName = pathname.Retrieve(11).Substring(3);
                }
                ActiveDirectorySubnet subnet = null;
                if (siteName == null)
                {
                    subnet = new ActiveDirectorySubnet(context, subnetName, null, true);
                }
                else
                {
                    subnet = new ActiveDirectorySubnet(context, subnetName, siteName, true);
                }
                subnet.cachedEntry = entry2;
                subnet2 = subnet;
            }
            catch (COMException exception3)
            {
                if (exception3.ErrorCode == -2147016656)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySubnet), subnetName);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception3);
            }
            finally
            {
                if (directoryEntry != null)
                {
                    directoryEntry.Dispose();
                }
            }
            return subnet2;
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            if (!this.existing)
            {
                throw new InvalidOperationException(Res.GetString("CannotGetObject"));
            }
            return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.cachedEntry.Path);
        }

        public void Save()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            try
            {
                if (this.existing)
                {
                    if (this.site == null)
                    {
                        if (this.cachedEntry.Properties.Contains("siteObject"))
                        {
                            this.cachedEntry.Properties["siteObject"].Clear();
                        }
                    }
                    else
                    {
                        this.cachedEntry.Properties["siteObject"].Value = this.site.cachedEntry.Properties["distinguishedName"][0];
                    }
                    this.cachedEntry.CommitChanges();
                }
                else
                {
                    if (this.Site != null)
                    {
                        this.cachedEntry.Properties["siteObject"].Add(this.site.cachedEntry.Properties["distinguishedName"][0]);
                    }
                    this.cachedEntry.CommitChanges();
                    this.existing = true;
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        public override string ToString()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
            return this.Name;
        }

        private static void ValidateArgument(DirectoryContext context, string subnetName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
            }
            if (((context.Name != null) && !context.isRootDomain()) && (!context.isServer() && !context.isADAMConfigSet()))
            {
                throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
            }
            if (subnetName == null)
            {
                throw new ArgumentNullException("subnetName");
            }
            if (subnetName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "subnetName");
            }
        }

        public string Location
        {
            get
            {
                string str;
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (this.cachedEntry.Properties.Contains("location"))
                    {
                        return (string) this.cachedEntry.Properties["location"][0];
                    }
                    str = null;
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                return str;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                try
                {
                    if (value == null)
                    {
                        if (this.cachedEntry.Properties.Contains("location"))
                        {
                            this.cachedEntry.Properties["location"].Clear();
                        }
                    }
                    else
                    {
                        this.cachedEntry.Properties["location"].Value = value;
                    }
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
            }
        }

        public string Name
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.name;
            }
        }

        public ActiveDirectorySite Site
        {
            get
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                return this.site;
            }
            set
            {
                if (this.disposed)
                {
                    throw new ObjectDisposedException(base.GetType().Name);
                }
                if ((value != null) && !value.existing)
                {
                    throw new InvalidOperationException(Res.GetString("SiteNotCommitted", new object[] { value }));
                }
                this.site = value;
            }
        }
    }
}

