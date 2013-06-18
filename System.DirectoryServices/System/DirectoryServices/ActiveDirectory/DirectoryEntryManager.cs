namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
    internal class DirectoryEntryManager
    {
        private string bindingPrefix;
        private DirectoryContext context;
        private Hashtable directoryEntries = new Hashtable();
        private NativeComInterfaces.IAdsPathname pathCracker;

        internal DirectoryEntryManager(DirectoryContext context)
        {
            this.context = context;
            this.pathCracker = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            this.pathCracker.EscapedMode = 2;
        }

        internal static DirectoryEntry Bind(string ldapPath, string username, string password, bool useServerBind)
        {
            AuthenticationTypes defaultAuthType = Utils.DefaultAuthType;
            if (DirectoryContext.ServerBindSupported && useServerBind)
            {
                defaultAuthType |= AuthenticationTypes.ServerBind;
            }
            return new DirectoryEntry(ldapPath, username, password, defaultAuthType);
        }

        internal string ExpandWellKnownDN(WellKnownDN dn)
        {
            switch (dn)
            {
                case WellKnownDN.RootDSE:
                    return "RootDSE";

                case WellKnownDN.DefaultNamingContext:
                {
                    DirectoryEntry cachedDirectoryEntry = this.GetCachedDirectoryEntry("RootDSE");
                    return (string) PropertyManager.GetPropertyValue(this.context, cachedDirectoryEntry, PropertyManager.DefaultNamingContext);
                }
                case WellKnownDN.SchemaNamingContext:
                {
                    DirectoryEntry directoryEntry = this.GetCachedDirectoryEntry("RootDSE");
                    return (string) PropertyManager.GetPropertyValue(this.context, directoryEntry, PropertyManager.SchemaNamingContext);
                }
                case WellKnownDN.ConfigurationNamingContext:
                {
                    DirectoryEntry entry4 = this.GetCachedDirectoryEntry("RootDSE");
                    return (string) PropertyManager.GetPropertyValue(this.context, entry4, PropertyManager.ConfigurationNamingContext);
                }
                case WellKnownDN.PartitionsContainer:
                    return ("CN=Partitions," + this.ExpandWellKnownDN(WellKnownDN.ConfigurationNamingContext));

                case WellKnownDN.SitesContainer:
                    return ("CN=Sites," + this.ExpandWellKnownDN(WellKnownDN.ConfigurationNamingContext));

                case WellKnownDN.SystemContainer:
                    return ("CN=System," + this.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));

                case WellKnownDN.RidManager:
                    return ("CN=RID Manager$," + this.ExpandWellKnownDN(WellKnownDN.SystemContainer));

                case WellKnownDN.Infrastructure:
                    return ("CN=Infrastructure," + this.ExpandWellKnownDN(WellKnownDN.DefaultNamingContext));

                case WellKnownDN.RootDomainNamingContext:
                {
                    DirectoryEntry entry = this.GetCachedDirectoryEntry("RootDSE");
                    return (string) PropertyManager.GetPropertyValue(this.context, entry, PropertyManager.RootDomainNamingContext);
                }
            }
            throw new InvalidEnumArgumentException("dn", (int) dn, typeof(WellKnownDN));
        }

        internal static string ExpandWellKnownDN(DirectoryContext context, WellKnownDN dn)
        {
            switch (dn)
            {
                case WellKnownDN.RootDSE:
                    return "RootDSE";

                case WellKnownDN.DefaultNamingContext:
                {
                    using (DirectoryEntry entry2 = GetDirectoryEntry(context, "RootDSE"))
                    {
                        return (string) PropertyManager.GetPropertyValue(context, entry2, PropertyManager.DefaultNamingContext);
                    }
                }
                case WellKnownDN.SchemaNamingContext:
                {
                    using (DirectoryEntry entry3 = GetDirectoryEntry(context, "RootDSE"))
                    {
                        return (string) PropertyManager.GetPropertyValue(context, entry3, PropertyManager.SchemaNamingContext);
                    }
                }
                case WellKnownDN.ConfigurationNamingContext:
                {
                    using (DirectoryEntry entry4 = GetDirectoryEntry(context, "RootDSE"))
                    {
                        return (string) PropertyManager.GetPropertyValue(context, entry4, PropertyManager.ConfigurationNamingContext);
                    }
                }
                case WellKnownDN.PartitionsContainer:
                    return ("CN=Partitions," + ExpandWellKnownDN(context, WellKnownDN.ConfigurationNamingContext));

                case WellKnownDN.SitesContainer:
                    return ("CN=Sites," + ExpandWellKnownDN(context, WellKnownDN.ConfigurationNamingContext));

                case WellKnownDN.SystemContainer:
                    return ("CN=System," + ExpandWellKnownDN(context, WellKnownDN.DefaultNamingContext));

                case WellKnownDN.RidManager:
                    return ("CN=RID Manager$," + ExpandWellKnownDN(context, WellKnownDN.SystemContainer));

                case WellKnownDN.Infrastructure:
                    return ("CN=Infrastructure," + ExpandWellKnownDN(context, WellKnownDN.DefaultNamingContext));

                case WellKnownDN.RootDomainNamingContext:
                {
                    using (DirectoryEntry entry = GetDirectoryEntry(context, "RootDSE"))
                    {
                        return (string) PropertyManager.GetPropertyValue(context, entry, PropertyManager.RootDomainNamingContext);
                    }
                }
            }
            throw new InvalidEnumArgumentException("dn", (int) dn, typeof(WellKnownDN));
        }

        internal ICollection GetCachedDirectoryEntries()
        {
            return this.directoryEntries.Values;
        }

        internal DirectoryEntry GetCachedDirectoryEntry(WellKnownDN dn)
        {
            return this.GetCachedDirectoryEntry(this.ExpandWellKnownDN(dn));
        }

        internal DirectoryEntry GetCachedDirectoryEntry(string distinguishedName)
        {
            object key = distinguishedName;
            if ((string.Compare(distinguishedName, "rootdse", StringComparison.OrdinalIgnoreCase) != 0) && (string.Compare(distinguishedName, "schema", StringComparison.OrdinalIgnoreCase) != 0))
            {
                key = new DistinguishedName(distinguishedName);
            }
            if (!this.directoryEntries.ContainsKey(key))
            {
                DirectoryEntry newDirectoryEntry = this.GetNewDirectoryEntry(distinguishedName);
                this.directoryEntries.Add(key, newDirectoryEntry);
            }
            return (DirectoryEntry) this.directoryEntries[key];
        }

        internal static DirectoryEntry GetDirectoryEntry(DirectoryContext context, WellKnownDN dn)
        {
            return GetDirectoryEntry(context, ExpandWellKnownDN(context, dn));
        }

        internal static DirectoryEntry GetDirectoryEntry(DirectoryContext context, string dn)
        {
            string str = "LDAP://" + context.GetServerName() + "/";
            NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname) new NativeComInterfaces.Pathname();
            pathname.EscapedMode = 2;
            pathname.Set(dn, 4);
            string str2 = pathname.Retrieve(7);
            return Bind(str + str2, context.UserName, context.Password, context.useServerBind());
        }

        internal static DirectoryEntry GetDirectoryEntryInternal(DirectoryContext context, string path)
        {
            return Bind(path, context.UserName, context.Password, context.useServerBind());
        }

        private DirectoryEntry GetNewDirectoryEntry(string dn)
        {
            if (this.bindingPrefix == null)
            {
                this.bindingPrefix = "LDAP://" + this.context.GetServerName() + "/";
            }
            this.pathCracker.Set(dn, 4);
            string str = this.pathCracker.Retrieve(7);
            return Bind(this.bindingPrefix + str, this.context.UserName, this.context.Password, this.context.useServerBind());
        }

        internal void RemoveIfExists(string distinguishedName)
        {
            object key = distinguishedName;
            if (string.Compare(distinguishedName, "rootdse", StringComparison.OrdinalIgnoreCase) != 0)
            {
                key = new DistinguishedName(distinguishedName);
            }
            if (this.directoryEntries.ContainsKey(key))
            {
                DirectoryEntry entry = (DirectoryEntry) this.directoryEntries[key];
                if (entry != null)
                {
                    this.directoryEntries.Remove(key);
                    entry.Dispose();
                }
            }
        }
    }
}

