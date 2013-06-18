namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectorySchemaProperty : IDisposable
    {
        private DirectoryEntry abstractPropertyEntry;
        private static OMObjectClass accessPointDnOMObjectClass = new OMObjectClass(new byte[] { 0x2b, 12, 2, 0x87, 0x73, 0x1c, 0, 0x85, 0x3e });
        private string commonName;
        private DirectoryContext context;
        private string description;
        private bool descriptionInitialized;
        private bool disposed;
        private static OMObjectClass dnOMObjectClass = new OMObjectClass(new byte[] { 0x2b, 12, 2, 0x87, 0x73, 0x1c, 0, 0x85, 0x4a });
        private static OMObjectClass dNWithBinaryOMObjectClass = new OMObjectClass(new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 20, 1, 1, 1, 11 });
        private static OMObjectClass dNWithStringOMObjectClass = new OMObjectClass(new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 20, 1, 1, 1, 12 });
        private NativeComInterfaces.IAdsProperty iadsProperty;
        internal bool isBound;
        private bool isDefunct;
        private bool isDefunctOnServer;
        private bool isInGlobalCatalog;
        private bool isInGlobalCatalogInitialized;
        private bool isSingleValued;
        private bool isSingleValuedInitialized;
        private string ldapDisplayName;
        private ActiveDirectorySchemaProperty linkedProperty;
        private bool linkedPropertyInitialized;
        private int? linkId;
        private bool linkIdInitialized;
        private string oid;
        private static OMObjectClass oRNameOMObjectClass = new OMObjectClass(new byte[] { 0x56, 6, 1, 2, 5, 11, 0x1d });
        private static OMObjectClass presentationAddressOMObjectClass = new OMObjectClass(new byte[] { 0x2b, 12, 2, 0x87, 0x73, 0x1c, 0, 0x85, 0x5c });
        private bool propertiesFromSchemaContainerInitialized;
        private DirectoryEntry propertyEntry;
        private SearchResult propertyValuesFromServer;
        private int? rangeLower;
        private bool rangeLowerInitialized;
        private int? rangeUpper;
        private bool rangeUpperInitialized;
        private static OMObjectClass replicaLinkOMObjectClass = new OMObjectClass(new byte[] { 0x2a, 0x86, 0x48, 0x86, 0xf7, 20, 1, 1, 1, 6 });
        private ActiveDirectorySchema schema;
        private DirectoryEntry schemaEntry;
        private byte[] schemaGuidBinaryForm;
        private SearchFlags searchFlags;
        private bool searchFlagsInitialized;
        private ActiveDirectorySyntax syntax;
        private static System.DirectoryServices.ActiveDirectory.Syntax[] syntaxes = new System.DirectoryServices.ActiveDirectory.Syntax[] { 
            new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.3", 0x1b, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.4", 20, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.6", 0x12, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.12", 0x40, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.10", 4, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.15", 0x42, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.9", 2, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.16", 0x41, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.8", 1, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.2", 6, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.11", 0x18, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.11", 0x17, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.1", 0x7f, dnOMObjectClass), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.7", 0x7f, dNWithBinaryOMObjectClass), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.14", 0x7f, dNWithStringOMObjectClass), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.9", 10, null), 
            new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.5", 0x16, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.5", 0x13, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.17", 4, null), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.14", 0x7f, accessPointDnOMObjectClass), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.7", 0x7f, oRNameOMObjectClass), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.13", 0x7f, presentationAddressOMObjectClass), new System.DirectoryServices.ActiveDirectory.Syntax("2.5.5.10", 0x7f, replicaLinkOMObjectClass)
         };
        private static int SyntaxesCount = 0x17;
        private bool syntaxInitialized;

        public ActiveDirectorySchemaProperty(DirectoryContext context, string ldapDisplayName)
        {
            this.syntax = ~ActiveDirectorySyntax.CaseExactString;
            this.rangeLower = null;
            this.rangeUpper = null;
            this.linkId = null;
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
            }
            if (((context.Name != null) && !context.isRootDomain()) && (!context.isADAMConfigSet() && !context.isServer()))
            {
                throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
            }
            if (ldapDisplayName == null)
            {
                throw new ArgumentNullException("ldapDisplayName");
            }
            if (ldapDisplayName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "ldapDisplayName");
            }
            this.context = new DirectoryContext(context);
            this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.SchemaNamingContext);
            this.schemaEntry.Bind(true);
            this.ldapDisplayName = ldapDisplayName;
            this.commonName = ldapDisplayName;
            this.isBound = false;
        }

        internal ActiveDirectorySchemaProperty(DirectoryContext context, string ldapDisplayName, DirectoryEntry propertyEntry, DirectoryEntry schemaEntry)
        {
            this.syntax = ~ActiveDirectorySyntax.CaseExactString;
            this.rangeLower = null;
            this.rangeUpper = null;
            this.linkId = null;
            this.context = context;
            this.ldapDisplayName = ldapDisplayName;
            this.propertyEntry = propertyEntry;
            this.isDefunctOnServer = false;
            this.isDefunct = this.isDefunctOnServer;
            try
            {
                this.abstractPropertyEntry = DirectoryEntryManager.GetDirectoryEntryInternal(context, "LDAP://" + context.GetServerName() + "/schema/" + ldapDisplayName);
                this.iadsProperty = (NativeComInterfaces.IAdsProperty) this.abstractPropertyEntry.NativeObject;
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147463168)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), ldapDisplayName);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (InvalidCastException)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), ldapDisplayName);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
            this.isBound = true;
        }

        internal ActiveDirectorySchemaProperty(DirectoryContext context, string commonName, SearchResult propertyValuesFromServer, DirectoryEntry schemaEntry)
        {
            this.syntax = ~ActiveDirectorySyntax.CaseExactString;
            this.rangeLower = null;
            this.rangeUpper = null;
            this.linkId = null;
            this.context = context;
            this.schemaEntry = schemaEntry;
            this.propertyValuesFromServer = propertyValuesFromServer;
            this.propertiesFromSchemaContainerInitialized = true;
            this.propertyEntry = this.GetSchemaPropertyDirectoryEntry();
            this.commonName = commonName;
            this.ldapDisplayName = (string) this.GetValueFromCache(PropertyManager.LdapDisplayName, true);
            this.isDefunctOnServer = true;
            this.isDefunct = this.isDefunctOnServer;
            this.isBound = true;
        }

        internal ActiveDirectorySchemaProperty(DirectoryContext context, string commonName, string ldapDisplayName, DirectoryEntry propertyEntry, DirectoryEntry schemaEntry)
        {
            this.syntax = ~ActiveDirectorySyntax.CaseExactString;
            this.rangeLower = null;
            this.rangeUpper = null;
            this.linkId = null;
            this.context = context;
            this.schemaEntry = schemaEntry;
            this.propertyEntry = propertyEntry;
            this.commonName = commonName;
            this.ldapDisplayName = ldapDisplayName;
            this.isDefunctOnServer = true;
            this.isDefunct = this.isDefunctOnServer;
            this.isBound = true;
        }

        private void CheckIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(base.GetType().Name);
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (this.schemaEntry != null)
                    {
                        this.schemaEntry.Dispose();
                        this.schemaEntry = null;
                    }
                    if (this.propertyEntry != null)
                    {
                        this.propertyEntry.Dispose();
                        this.propertyEntry = null;
                    }
                    if (this.abstractPropertyEntry != null)
                    {
                        this.abstractPropertyEntry.Dispose();
                        this.abstractPropertyEntry = null;
                    }
                    if (this.schema != null)
                    {
                        this.schema.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        public static ActiveDirectorySchemaProperty FindByName(DirectoryContext context, string ldapDisplayName)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if ((context.Name == null) && !context.isRootDomain())
            {
                throw new ArgumentException(Res.GetString("ContextNotAssociatedWithDomain"), "context");
            }
            if (((context.Name != null) && !context.isRootDomain()) && (!context.isADAMConfigSet() && !context.isServer()))
            {
                throw new ArgumentException(Res.GetString("NotADOrADAM"), "context");
            }
            if (ldapDisplayName == null)
            {
                throw new ArgumentNullException("ldapDisplayName");
            }
            if (ldapDisplayName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "ldapDisplayName");
            }
            context = new DirectoryContext(context);
            return new ActiveDirectorySchemaProperty(context, ldapDisplayName, null, null);
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            this.CheckIfDisposed();
            if (!this.isBound)
            {
                throw new InvalidOperationException(Res.GetString("CannotGetObject"));
            }
            this.GetSchemaPropertyDirectoryEntry();
            return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.propertyEntry.Path);
        }

        internal static SearchResult GetPropertiesFromSchemaContainer(DirectoryContext context, DirectoryEntry schemaEntry, string name, bool isDefunctOnServer)
        {
            SearchResult result = null;
            StringBuilder builder = new StringBuilder(15);
            builder.Append("(&(");
            builder.Append(PropertyManager.ObjectCategory);
            builder.Append("=attributeSchema)");
            builder.Append("(");
            if (!isDefunctOnServer)
            {
                builder.Append(PropertyManager.LdapDisplayName);
            }
            else
            {
                builder.Append(PropertyManager.Cn);
            }
            builder.Append("=");
            builder.Append(Utils.GetEscapedFilterValue(name));
            builder.Append(")");
            if (!isDefunctOnServer)
            {
                builder.Append("(!(");
            }
            else
            {
                builder.Append("(");
            }
            builder.Append(PropertyManager.IsDefunct);
            if (!isDefunctOnServer)
            {
                builder.Append("=TRUE)))");
            }
            else
            {
                builder.Append("=TRUE))");
            }
            string[] propertiesToLoad = null;
            if (!isDefunctOnServer)
            {
                propertiesToLoad = new string[] { PropertyManager.DistinguishedName, PropertyManager.Cn, PropertyManager.AttributeSyntax, PropertyManager.OMSyntax, PropertyManager.OMObjectClass, PropertyManager.Description, PropertyManager.SearchFlags, PropertyManager.IsMemberOfPartialAttributeSet, PropertyManager.LinkID, PropertyManager.SchemaIDGuid, PropertyManager.RangeLower, PropertyManager.RangeUpper };
            }
            else
            {
                propertiesToLoad = new string[] { PropertyManager.DistinguishedName, PropertyManager.Cn, PropertyManager.AttributeSyntax, PropertyManager.OMSyntax, PropertyManager.OMObjectClass, PropertyManager.Description, PropertyManager.SearchFlags, PropertyManager.IsMemberOfPartialAttributeSet, PropertyManager.LinkID, PropertyManager.SchemaIDGuid, PropertyManager.AttributeID, PropertyManager.IsSingleValued, PropertyManager.RangeLower, PropertyManager.RangeUpper, PropertyManager.LdapDisplayName };
            }
            ADSearcher searcher = new ADSearcher(schemaEntry, builder.ToString(), propertiesToLoad, SearchScope.OneLevel, false, false);
            try
            {
                result = searcher.FindOne();
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016656)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), name);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            if (result == null)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaProperty), name);
            }
            return result;
        }

        internal DirectoryEntry GetSchemaPropertyDirectoryEntry()
        {
            if (this.propertyEntry == null)
            {
                this.InitializePropertiesFromSchemaContainer();
                this.propertyEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, (string) this.GetValueFromCache(PropertyManager.DistinguishedName, true));
            }
            return this.propertyEntry;
        }

        private object GetValueFromCache(string propertyName, bool mustExist)
        {
            object obj2 = null;
            this.InitializePropertiesFromSchemaContainer();
            ResultPropertyValueCollection values = null;
            try
            {
                values = this.propertyValuesFromServer.Properties[propertyName];
                if ((values == null) || (values.Count < 1))
                {
                    if (mustExist)
                    {
                        throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", new object[] { propertyName }));
                    }
                    return obj2;
                }
                obj2 = values[0];
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            return obj2;
        }

        private void InitializePropertiesFromSchemaContainer()
        {
            if (!this.propertiesFromSchemaContainerInitialized)
            {
                if (this.schemaEntry == null)
                {
                    this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
                }
                this.propertyValuesFromServer = GetPropertiesFromSchemaContainer(this.context, this.schemaEntry, this.isDefunctOnServer ? this.commonName : this.ldapDisplayName, this.isDefunctOnServer);
                this.propertiesFromSchemaContainerInitialized = true;
            }
        }

        private void InitializeSearchFlags()
        {
            if (this.isBound && !this.searchFlagsInitialized)
            {
                object valueFromCache = this.GetValueFromCache(PropertyManager.SearchFlags, false);
                if (valueFromCache != null)
                {
                    this.searchFlags = (SearchFlags) ((int) valueFromCache);
                }
                this.searchFlagsInitialized = true;
            }
        }

        private bool IsSetInSearchFlags(SearchFlags searchFlagBit)
        {
            this.InitializeSearchFlags();
            return ((this.searchFlags & searchFlagBit) != SearchFlags.None);
        }

        private ActiveDirectorySyntax MapSyntax(string syntaxId, int oMID, OMObjectClass oMObjectClass)
        {
            for (int i = 0; i < SyntaxesCount; i++)
            {
                if (syntaxes[i].Equals(new System.DirectoryServices.ActiveDirectory.Syntax(syntaxId, oMID, oMObjectClass)))
                {
                    return (ActiveDirectorySyntax) i;
                }
            }
            throw new ActiveDirectoryOperationException(Res.GetString("UnknownSyntax", new object[] { this.ldapDisplayName }));
        }

        private void ResetBitInSearchFlags(SearchFlags searchFlagBit)
        {
            this.InitializeSearchFlags();
            this.searchFlags &= ~searchFlagBit;
            if (this.isBound)
            {
                this.GetSchemaPropertyDirectoryEntry();
                this.propertyEntry.Properties[PropertyManager.SearchFlags].Value = (int) this.searchFlags;
            }
        }

        public void Save()
        {
            this.CheckIfDisposed();
            if (!this.isBound)
            {
                try
                {
                    if (this.schemaEntry == null)
                    {
                        this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
                    }
                    string escapedPath = Utils.GetEscapedPath("CN=" + this.commonName);
                    this.propertyEntry = this.schemaEntry.Children.Add(escapedPath, "attributeSchema");
                }
                catch (COMException exception)
                {
                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                }
                catch (ActiveDirectoryObjectNotFoundException)
                {
                    throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { this.context.Name }));
                }
                this.SetProperty(PropertyManager.LdapDisplayName, this.ldapDisplayName);
                this.SetProperty(PropertyManager.AttributeID, this.oid);
                if (this.syntax != ~ActiveDirectorySyntax.CaseExactString)
                {
                    this.SetSyntax(this.syntax);
                }
                this.SetProperty(PropertyManager.Description, this.description);
                this.propertyEntry.Properties[PropertyManager.IsSingleValued].Value = this.isSingleValued;
                this.propertyEntry.Properties[PropertyManager.IsMemberOfPartialAttributeSet].Value = this.isInGlobalCatalog;
                this.propertyEntry.Properties[PropertyManager.IsDefunct].Value = this.isDefunct;
                if (this.rangeLower.HasValue)
                {
                    this.propertyEntry.Properties[PropertyManager.RangeLower].Value = this.rangeLower.Value;
                }
                if (this.rangeUpper.HasValue)
                {
                    this.propertyEntry.Properties[PropertyManager.RangeUpper].Value = this.rangeUpper.Value;
                }
                if (this.searchFlags != SearchFlags.None)
                {
                    this.propertyEntry.Properties[PropertyManager.SearchFlags].Value = (int) this.searchFlags;
                }
                if (this.linkId.HasValue)
                {
                    this.propertyEntry.Properties[PropertyManager.LinkID].Value = this.linkId.Value;
                }
                if (this.schemaGuidBinaryForm != null)
                {
                    this.SetProperty(PropertyManager.SchemaIDGuid, this.schemaGuidBinaryForm);
                }
            }
            try
            {
                this.propertyEntry.CommitChanges();
                if (this.schema == null)
                {
                    ActiveDirectorySchema schema = ActiveDirectorySchema.GetSchema(this.context);
                    bool flag = false;
                    DirectoryServer schemaRoleOwner = null;
                    try
                    {
                        schemaRoleOwner = schema.SchemaRoleOwner;
                        if (Utils.Compare(schemaRoleOwner.Name, this.context.GetServerName()) != 0)
                        {
                            DirectoryContext context = Utils.GetNewDirectoryContext(schemaRoleOwner.Name, DirectoryContextType.DirectoryServer, this.context);
                            this.schema = ActiveDirectorySchema.GetSchema(context);
                        }
                        else
                        {
                            flag = true;
                            this.schema = schema;
                        }
                    }
                    finally
                    {
                        if (schemaRoleOwner != null)
                        {
                            schemaRoleOwner.Dispose();
                        }
                        if (!flag)
                        {
                            schema.Dispose();
                        }
                    }
                }
                this.schema.RefreshSchema();
            }
            catch (COMException exception2)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception2);
            }
            this.isDefunctOnServer = this.isDefunct;
            this.commonName = null;
            this.oid = null;
            this.syntaxInitialized = false;
            this.descriptionInitialized = false;
            this.isSingleValuedInitialized = false;
            this.isInGlobalCatalogInitialized = false;
            this.rangeLowerInitialized = false;
            this.rangeUpperInitialized = false;
            this.searchFlagsInitialized = false;
            this.linkedPropertyInitialized = false;
            this.linkIdInitialized = false;
            this.schemaGuidBinaryForm = null;
            this.propertiesFromSchemaContainerInitialized = false;
            this.isBound = true;
        }

        private void SetBitInSearchFlags(SearchFlags searchFlagBit)
        {
            this.InitializeSearchFlags();
            this.searchFlags |= searchFlagBit;
            if (this.isBound)
            {
                this.GetSchemaPropertyDirectoryEntry();
                this.propertyEntry.Properties[PropertyManager.SearchFlags].Value = (int) this.searchFlags;
            }
        }

        private void SetProperty(string propertyName, object value)
        {
            this.GetSchemaPropertyDirectoryEntry();
            if (value == null)
            {
                if (this.propertyEntry.Properties.Contains(propertyName))
                {
                    this.propertyEntry.Properties[propertyName].Clear();
                }
            }
            else
            {
                this.propertyEntry.Properties[propertyName].Value = value;
            }
        }

        private void SetSyntax(ActiveDirectorySyntax syntax)
        {
            if ((syntax < ActiveDirectorySyntax.CaseExactString) || (syntax > (SyntaxesCount - 1)))
            {
                throw new InvalidEnumArgumentException("syntax", (int) syntax, typeof(ActiveDirectorySyntax));
            }
            this.GetSchemaPropertyDirectoryEntry();
            this.propertyEntry.Properties[PropertyManager.AttributeSyntax].Value = syntaxes[(int) syntax].attributeSyntax;
            this.propertyEntry.Properties[PropertyManager.OMSyntax].Value = syntaxes[(int) syntax].oMSyntax;
            OMObjectClass oMObjectClass = syntaxes[(int) syntax].oMObjectClass;
            if (oMObjectClass != null)
            {
                this.propertyEntry.Properties[PropertyManager.OMObjectClass].Value = oMObjectClass.Data;
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public string CommonName
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && (this.commonName == null))
                {
                    this.commonName = (string) this.GetValueFromCache(PropertyManager.Cn, true);
                }
                return this.commonName;
            }
            set
            {
                this.CheckIfDisposed();
                if ((value != null) && (value.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("EmptyStringParameter"), "value");
                }
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.Cn, value);
                }
                this.commonName = value;
            }
        }

        public string Description
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.descriptionInitialized)
                {
                    this.description = (string) this.GetValueFromCache(PropertyManager.Description, false);
                    this.descriptionInitialized = true;
                }
                return this.description;
            }
            set
            {
                this.CheckIfDisposed();
                if ((value != null) && (value.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("EmptyStringParameter"), "value");
                }
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.Description, value);
                }
                this.description = value;
            }
        }

        public bool IsDefunct
        {
            get
            {
                this.CheckIfDisposed();
                return this.isDefunct;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.IsDefunct, value);
                }
                this.isDefunct = value;
            }
        }

        public bool IsInAnr
        {
            get
            {
                this.CheckIfDisposed();
                return this.IsSetInSearchFlags(SearchFlags.IsInAnr);
            }
            set
            {
                this.CheckIfDisposed();
                if (value)
                {
                    this.SetBitInSearchFlags(SearchFlags.IsInAnr);
                }
                else
                {
                    this.ResetBitInSearchFlags(SearchFlags.IsInAnr);
                }
            }
        }

        public bool IsIndexed
        {
            get
            {
                this.CheckIfDisposed();
                return this.IsSetInSearchFlags(SearchFlags.IsIndexed);
            }
            set
            {
                this.CheckIfDisposed();
                if (value)
                {
                    this.SetBitInSearchFlags(SearchFlags.IsIndexed);
                }
                else
                {
                    this.ResetBitInSearchFlags(SearchFlags.IsIndexed);
                }
            }
        }

        public bool IsIndexedOverContainer
        {
            get
            {
                this.CheckIfDisposed();
                return this.IsSetInSearchFlags(SearchFlags.IsIndexedOverContainer);
            }
            set
            {
                this.CheckIfDisposed();
                if (value)
                {
                    this.SetBitInSearchFlags(SearchFlags.IsIndexedOverContainer);
                }
                else
                {
                    this.ResetBitInSearchFlags(SearchFlags.IsIndexedOverContainer);
                }
            }
        }

        public bool IsInGlobalCatalog
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.isInGlobalCatalogInitialized)
                {
                    object valueFromCache = this.GetValueFromCache(PropertyManager.IsMemberOfPartialAttributeSet, false);
                    this.isInGlobalCatalog = (valueFromCache != null) ? ((bool) valueFromCache) : false;
                    this.isInGlobalCatalogInitialized = true;
                }
                return this.isInGlobalCatalog;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.GetSchemaPropertyDirectoryEntry();
                    this.propertyEntry.Properties[PropertyManager.IsMemberOfPartialAttributeSet].Value = value;
                }
                this.isInGlobalCatalog = value;
            }
        }

        public bool IsOnTombstonedObject
        {
            get
            {
                this.CheckIfDisposed();
                return this.IsSetInSearchFlags(SearchFlags.IsOnTombstonedObject);
            }
            set
            {
                this.CheckIfDisposed();
                if (value)
                {
                    this.SetBitInSearchFlags(SearchFlags.IsOnTombstonedObject);
                }
                else
                {
                    this.ResetBitInSearchFlags(SearchFlags.IsOnTombstonedObject);
                }
            }
        }

        public bool IsSingleValued
        {
            get
            {
                this.CheckIfDisposed();
                if (!this.isBound || this.isSingleValuedInitialized)
                {
                    goto Label_0060;
                }
                if (!this.isDefunctOnServer)
                {
                    try
                    {
                        this.isSingleValued = !this.iadsProperty.MultiValued;
                        goto Label_0059;
                    }
                    catch (COMException exception)
                    {
                        throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                    }
                }
                this.isSingleValued = (bool) this.GetValueFromCache(PropertyManager.IsSingleValued, true);
            Label_0059:
                this.isSingleValuedInitialized = true;
            Label_0060:
                return this.isSingleValued;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.GetSchemaPropertyDirectoryEntry();
                    this.propertyEntry.Properties[PropertyManager.IsSingleValued].Value = value;
                }
                this.isSingleValued = value;
            }
        }

        public bool IsTupleIndexed
        {
            get
            {
                this.CheckIfDisposed();
                return this.IsSetInSearchFlags(SearchFlags.IsTupleIndexed);
            }
            set
            {
                this.CheckIfDisposed();
                if (value)
                {
                    this.SetBitInSearchFlags(SearchFlags.IsTupleIndexed);
                }
                else
                {
                    this.ResetBitInSearchFlags(SearchFlags.IsTupleIndexed);
                }
            }
        }

        public ActiveDirectorySchemaProperty Link
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.linkedPropertyInitialized)
                {
                    object valueFromCache = this.GetValueFromCache(PropertyManager.LinkID, false);
                    int num = (valueFromCache != null) ? ((int) valueFromCache) : -1;
                    if (num != -1)
                    {
                        int num2 = (num - (2 * (num % 2))) + 1;
                        try
                        {
                            if (this.schemaEntry == null)
                            {
                                this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
                            }
                            string filter = string.Concat(new object[] { "(&(", PropertyManager.ObjectCategory, "=attributeSchema)(", PropertyManager.LinkID, "=", num2, "))" });
                            ReadOnlyActiveDirectorySchemaPropertyCollection propertys = ActiveDirectorySchema.GetAllProperties(this.context, this.schemaEntry, filter);
                            if (propertys.Count != 1)
                            {
                                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("LinkedPropertyNotFound", new object[] { num2 }), typeof(ActiveDirectorySchemaProperty), null);
                            }
                            this.linkedProperty = propertys[0];
                        }
                        catch (COMException exception)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                        }
                    }
                    this.linkedPropertyInitialized = true;
                }
                return this.linkedProperty;
            }
        }

        public int? LinkId
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.linkIdInitialized)
                {
                    object valueFromCache = this.GetValueFromCache(PropertyManager.LinkID, false);
                    if (valueFromCache == null)
                    {
                        this.linkId = null;
                    }
                    else
                    {
                        this.linkId = new int?((int) valueFromCache);
                    }
                    this.linkIdInitialized = true;
                }
                return this.linkId;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.GetSchemaPropertyDirectoryEntry();
                    if (!value.HasValue)
                    {
                        if (this.propertyEntry.Properties.Contains(PropertyManager.LinkID))
                        {
                            this.propertyEntry.Properties[PropertyManager.LinkID].Clear();
                        }
                    }
                    else
                    {
                        this.propertyEntry.Properties[PropertyManager.LinkID].Value = value.Value;
                    }
                }
                this.linkId = value;
            }
        }

        public string Name
        {
            get
            {
                this.CheckIfDisposed();
                return this.ldapDisplayName;
            }
        }

        public string Oid
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && (this.oid == null))
                {
                    if (!this.isDefunctOnServer)
                    {
                        try
                        {
                            this.oid = this.iadsProperty.OID;
                            goto Label_0056;
                        }
                        catch (COMException exception)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                        }
                    }
                    this.oid = (string) this.GetValueFromCache(PropertyManager.AttributeID, true);
                }
            Label_0056:
                return this.oid;
            }
            set
            {
                this.CheckIfDisposed();
                if ((value != null) && (value.Length == 0))
                {
                    throw new ArgumentException(Res.GetString("EmptyStringParameter"), "value");
                }
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.AttributeID, value);
                }
                this.oid = value;
            }
        }

        public int? RangeLower
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.rangeLowerInitialized)
                {
                    object valueFromCache = this.GetValueFromCache(PropertyManager.RangeLower, false);
                    if (valueFromCache == null)
                    {
                        this.rangeLower = null;
                    }
                    else
                    {
                        this.rangeLower = new int?((int) valueFromCache);
                    }
                    this.rangeLowerInitialized = true;
                }
                return this.rangeLower;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.GetSchemaPropertyDirectoryEntry();
                    if (!value.HasValue)
                    {
                        if (this.propertyEntry.Properties.Contains(PropertyManager.RangeLower))
                        {
                            this.propertyEntry.Properties[PropertyManager.RangeLower].Clear();
                        }
                    }
                    else
                    {
                        this.propertyEntry.Properties[PropertyManager.RangeLower].Value = value.Value;
                    }
                }
                this.rangeLower = value;
            }
        }

        public int? RangeUpper
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.rangeUpperInitialized)
                {
                    object valueFromCache = this.GetValueFromCache(PropertyManager.RangeUpper, false);
                    if (valueFromCache == null)
                    {
                        this.rangeUpper = null;
                    }
                    else
                    {
                        this.rangeUpper = new int?((int) valueFromCache);
                    }
                    this.rangeUpperInitialized = true;
                }
                return this.rangeUpper;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.GetSchemaPropertyDirectoryEntry();
                    if (!value.HasValue)
                    {
                        if (this.propertyEntry.Properties.Contains(PropertyManager.RangeUpper))
                        {
                            this.propertyEntry.Properties[PropertyManager.RangeUpper].Clear();
                        }
                    }
                    else
                    {
                        this.propertyEntry.Properties[PropertyManager.RangeUpper].Value = value.Value;
                    }
                }
                this.rangeUpper = value;
            }
        }

        public Guid SchemaGuid
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && (this.schemaGuidBinaryForm == null))
                {
                    this.schemaGuidBinaryForm = (byte[]) this.GetValueFromCache(PropertyManager.SchemaIDGuid, true);
                }
                return new Guid(this.schemaGuidBinaryForm);
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.SchemaIDGuid, value.Equals(Guid.Empty) ? null : value.ToByteArray());
                }
                this.schemaGuidBinaryForm = value.Equals(Guid.Empty) ? null : value.ToByteArray();
            }
        }

        public ActiveDirectorySyntax Syntax
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.syntaxInitialized)
                {
                    byte[] valueFromCache = (byte[]) this.GetValueFromCache(PropertyManager.OMObjectClass, false);
                    OMObjectClass oMObjectClass = (valueFromCache != null) ? new OMObjectClass(valueFromCache) : null;
                    this.syntax = this.MapSyntax((string) this.GetValueFromCache(PropertyManager.AttributeSyntax, true), (int) this.GetValueFromCache(PropertyManager.OMSyntax, true), oMObjectClass);
                    this.syntaxInitialized = true;
                }
                return this.syntax;
            }
            set
            {
                this.CheckIfDisposed();
                if ((value < ActiveDirectorySyntax.CaseExactString) || (value > ActiveDirectorySyntax.ReplicaLink))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ActiveDirectorySyntax));
                }
                if (this.isBound)
                {
                    this.SetSyntax(value);
                }
                this.syntax = value;
            }
        }
    }
}

