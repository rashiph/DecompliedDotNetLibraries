namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.DirectoryServices;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Security.Permissions;
    using System.Text;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class ActiveDirectorySchemaClass : IDisposable
    {
        private DirectoryEntry abstractClassEntry;
        private ActiveDirectorySchemaClassCollection auxiliaryClasses;
        private DirectoryEntry classEntry;
        private string commonName;
        private DirectoryContext context;
        private string defaultSDSddlForm;
        private bool defaultSDSddlFormInitialized;
        private string description;
        private bool descriptionInitialized;
        private bool disposed;
        private NativeComInterfaces.IAdsClass iadsClass;
        internal bool isBound;
        private bool isDefunct;
        private bool isDefunctOnServer;
        private string ldapDisplayName;
        private ActiveDirectorySchemaPropertyCollection mandatoryProperties;
        private string oid;
        private ActiveDirectorySchemaPropertyCollection optionalProperties;
        private ReadOnlyActiveDirectorySchemaClassCollection possibleInferiors;
        private ActiveDirectorySchemaClassCollection possibleSuperiors;
        private bool propertiesFromSchemaContainerInitialized;
        private Hashtable propertyValuesFromServer;
        private ActiveDirectorySchema schema;
        private DirectoryEntry schemaEntry;
        private byte[] schemaGuidBinaryForm;
        private ActiveDirectorySchemaClass subClassOf;
        private SchemaClassType type;
        private bool typeInitialized;

        public ActiveDirectorySchemaClass(DirectoryContext context, string ldapDisplayName)
        {
            this.type = SchemaClassType.Structural;
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

        internal ActiveDirectorySchemaClass(DirectoryContext context, string commonName, Hashtable propertyValuesFromServer, DirectoryEntry schemaEntry)
        {
            this.type = SchemaClassType.Structural;
            this.context = context;
            this.schemaEntry = schemaEntry;
            this.propertyValuesFromServer = propertyValuesFromServer;
            this.propertiesFromSchemaContainerInitialized = true;
            this.classEntry = this.GetSchemaClassDirectoryEntry();
            this.commonName = commonName;
            this.ldapDisplayName = (string) this.GetValueFromCache(PropertyManager.LdapDisplayName, true);
            this.isDefunctOnServer = true;
            this.isDefunct = this.isDefunctOnServer;
            this.isBound = true;
        }

        internal ActiveDirectorySchemaClass(DirectoryContext context, string ldapDisplayName, DirectoryEntry classEntry, DirectoryEntry schemaEntry)
        {
            this.type = SchemaClassType.Structural;
            this.context = context;
            this.ldapDisplayName = ldapDisplayName;
            this.classEntry = classEntry;
            this.schemaEntry = schemaEntry;
            this.isDefunctOnServer = false;
            this.isDefunct = this.isDefunctOnServer;
            try
            {
                this.abstractClassEntry = DirectoryEntryManager.GetDirectoryEntryInternal(context, "LDAP://" + context.GetServerName() + "/schema/" + ldapDisplayName);
                this.iadsClass = (NativeComInterfaces.IAdsClass) this.abstractClassEntry.NativeObject;
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147463168)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaClass), ldapDisplayName);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            catch (InvalidCastException)
            {
                throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaClass), ldapDisplayName);
            }
            catch (ActiveDirectoryObjectNotFoundException)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("ADAMInstanceNotFoundInConfigSet", new object[] { context.Name }));
            }
            this.isBound = true;
        }

        internal ActiveDirectorySchemaClass(DirectoryContext context, string commonName, string ldapDisplayName, DirectoryEntry classEntry, DirectoryEntry schemaEntry)
        {
            this.type = SchemaClassType.Structural;
            this.context = context;
            this.schemaEntry = schemaEntry;
            this.classEntry = classEntry;
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
                    if (this.classEntry != null)
                    {
                        this.classEntry.Dispose();
                        this.classEntry = null;
                    }
                    if (this.abstractClassEntry != null)
                    {
                        this.abstractClassEntry.Dispose();
                        this.abstractClassEntry = null;
                    }
                    if (this.schema != null)
                    {
                        this.schema.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        public static ActiveDirectorySchemaClass FindByName(DirectoryContext context, string ldapDisplayName)
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
            if (ldapDisplayName == null)
            {
                throw new ArgumentNullException("ldapDisplayName");
            }
            if (ldapDisplayName.Length == 0)
            {
                throw new ArgumentException(Res.GetString("EmptyStringParameter"), "ldapDisplayName");
            }
            context = new DirectoryContext(context);
            return new ActiveDirectorySchemaClass(context, ldapDisplayName, null, null);
        }

        public ReadOnlyActiveDirectorySchemaPropertyCollection GetAllProperties()
        {
            this.CheckIfDisposed();
            ArrayList values = new ArrayList();
            values.AddRange(this.MandatoryProperties);
            values.AddRange(this.OptionalProperties);
            return new ReadOnlyActiveDirectorySchemaPropertyCollection(values);
        }

        private ArrayList GetClasses(ICollection ldapDisplayNames)
        {
            ArrayList list = new ArrayList();
            SearchResultCollection results = null;
            try
            {
                if (ldapDisplayNames.Count >= 1)
                {
                    if (this.schemaEntry == null)
                    {
                        this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
                    }
                    StringBuilder builder = new StringBuilder(100);
                    if (ldapDisplayNames.Count > 1)
                    {
                        builder.Append("(|");
                    }
                    foreach (string str in ldapDisplayNames)
                    {
                        builder.Append("(");
                        builder.Append(PropertyManager.LdapDisplayName);
                        builder.Append("=");
                        builder.Append(Utils.GetEscapedFilterValue(str));
                        builder.Append(")");
                    }
                    if (ldapDisplayNames.Count > 1)
                    {
                        builder.Append(")");
                    }
                    string filter = "(&(" + PropertyManager.ObjectCategory + "=classSchema)" + builder.ToString() + "(!(" + PropertyManager.IsDefunct + "=TRUE)))";
                    string[] propertiesToLoad = new string[] { PropertyManager.LdapDisplayName };
                    results = new ADSearcher(this.schemaEntry, filter, propertiesToLoad, SearchScope.OneLevel).FindAll();
                    foreach (SearchResult result in results)
                    {
                        string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.LdapDisplayName);
                        DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                        directoryEntry.AuthenticationType = Utils.DefaultAuthType;
                        directoryEntry.Username = this.context.UserName;
                        directoryEntry.Password = this.context.Password;
                        ActiveDirectorySchemaClass class2 = new ActiveDirectorySchemaClass(this.context, searchResultPropertyValue, directoryEntry, this.schemaEntry);
                        list.Add(class2);
                    }
                }
                return list;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
            }
            return list;
        }

        public DirectoryEntry GetDirectoryEntry()
        {
            this.CheckIfDisposed();
            if (!this.isBound)
            {
                throw new InvalidOperationException(Res.GetString("CannotGetObject"));
            }
            this.GetSchemaClassDirectoryEntry();
            return DirectoryEntryManager.GetDirectoryEntryInternal(this.context, this.classEntry.Path);
        }

        private ArrayList GetProperties(ICollection ldapDisplayNames)
        {
            ArrayList list = new ArrayList();
            SearchResultCollection results = null;
            try
            {
                if (ldapDisplayNames.Count >= 1)
                {
                    if (this.schemaEntry == null)
                    {
                        this.schemaEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, WellKnownDN.SchemaNamingContext);
                    }
                    StringBuilder builder = new StringBuilder(100);
                    if (ldapDisplayNames.Count > 1)
                    {
                        builder.Append("(|");
                    }
                    foreach (string str in ldapDisplayNames)
                    {
                        builder.Append("(");
                        builder.Append(PropertyManager.LdapDisplayName);
                        builder.Append("=");
                        builder.Append(Utils.GetEscapedFilterValue(str));
                        builder.Append(")");
                    }
                    if (ldapDisplayNames.Count > 1)
                    {
                        builder.Append(")");
                    }
                    string filter = "(&(" + PropertyManager.ObjectCategory + "=attributeSchema)" + builder.ToString() + "(!(" + PropertyManager.IsDefunct + "=TRUE)))";
                    string[] propertiesToLoad = new string[] { PropertyManager.LdapDisplayName };
                    results = new ADSearcher(this.schemaEntry, filter, propertiesToLoad, SearchScope.OneLevel).FindAll();
                    foreach (SearchResult result in results)
                    {
                        string searchResultPropertyValue = (string) PropertyManager.GetSearchResultPropertyValue(result, PropertyManager.LdapDisplayName);
                        DirectoryEntry directoryEntry = result.GetDirectoryEntry();
                        directoryEntry.AuthenticationType = Utils.DefaultAuthType;
                        directoryEntry.Username = this.context.UserName;
                        directoryEntry.Password = this.context.Password;
                        ActiveDirectorySchemaProperty property = new ActiveDirectorySchemaProperty(this.context, searchResultPropertyValue, directoryEntry, this.schemaEntry);
                        list.Add(property);
                    }
                }
                return list;
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            finally
            {
                if (results != null)
                {
                    results.Dispose();
                }
            }
            return list;
        }

        internal static Hashtable GetPropertiesFromSchemaContainer(DirectoryContext context, DirectoryEntry schemaEntry, string name, bool isDefunctOnServer)
        {
            Hashtable hashtable = null;
            StringBuilder builder = new StringBuilder(15);
            builder.Append("(&(");
            builder.Append(PropertyManager.ObjectCategory);
            builder.Append("=classSchema)");
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
            ArrayList propertiesWithRangeRetrieval = new ArrayList();
            ArrayList propertiesWithoutRangeRetrieval = new ArrayList();
            propertiesWithoutRangeRetrieval.Add(PropertyManager.DistinguishedName);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.Cn);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.Description);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.PossibleInferiors);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.SubClassOf);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.ObjectClassCategory);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.SchemaIDGuid);
            propertiesWithoutRangeRetrieval.Add(PropertyManager.DefaultSecurityDescriptor);
            propertiesWithRangeRetrieval.Add(PropertyManager.AuxiliaryClass);
            propertiesWithRangeRetrieval.Add(PropertyManager.SystemAuxiliaryClass);
            propertiesWithRangeRetrieval.Add(PropertyManager.MustContain);
            propertiesWithRangeRetrieval.Add(PropertyManager.SystemMustContain);
            propertiesWithRangeRetrieval.Add(PropertyManager.MayContain);
            propertiesWithRangeRetrieval.Add(PropertyManager.SystemMayContain);
            if (isDefunctOnServer)
            {
                propertiesWithoutRangeRetrieval.Add(PropertyManager.LdapDisplayName);
                propertiesWithoutRangeRetrieval.Add(PropertyManager.GovernsID);
                propertiesWithRangeRetrieval.Add(PropertyManager.SystemPossibleSuperiors);
                propertiesWithRangeRetrieval.Add(PropertyManager.PossibleSuperiors);
            }
            try
            {
                hashtable = Utils.GetValuesWithRangeRetrieval(schemaEntry, builder.ToString(), propertiesWithRangeRetrieval, propertiesWithoutRangeRetrieval, SearchScope.OneLevel);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147016656)
                {
                    throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"), typeof(ActiveDirectorySchemaClass), name);
                }
                throw ExceptionHelper.GetExceptionFromCOMException(context, exception);
            }
            return hashtable;
        }

        private ArrayList GetPropertyValuesRecursively(string[] propertyNames)
        {
            ArrayList list = new ArrayList();
            try
            {
                if (Utils.Compare(this.SubClassOf.Name, this.Name) != 0)
                {
                    foreach (string str in this.SubClassOf.GetPropertyValuesRecursively(propertyNames))
                    {
                        if (!list.Contains(str))
                        {
                            list.Add(str);
                        }
                    }
                }
                foreach (string str2 in this.GetValuesFromCache(PropertyManager.AuxiliaryClass))
                {
                    ActiveDirectorySchemaClass class2 = new ActiveDirectorySchemaClass(this.context, str2, null, null);
                    foreach (string str3 in class2.GetPropertyValuesRecursively(propertyNames))
                    {
                        if (!list.Contains(str3))
                        {
                            list.Add(str3);
                        }
                    }
                }
                foreach (string str4 in this.GetValuesFromCache(PropertyManager.SystemAuxiliaryClass))
                {
                    ActiveDirectorySchemaClass class3 = new ActiveDirectorySchemaClass(this.context, str4, null, null);
                    foreach (string str5 in class3.GetPropertyValuesRecursively(propertyNames))
                    {
                        if (!list.Contains(str5))
                        {
                            list.Add(str5);
                        }
                    }
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
            foreach (string str6 in propertyNames)
            {
                foreach (string str7 in this.GetValuesFromCache(str6))
                {
                    if (!list.Contains(str7))
                    {
                        list.Add(str7);
                    }
                }
            }
            return list;
        }

        internal DirectoryEntry GetSchemaClassDirectoryEntry()
        {
            if (this.classEntry == null)
            {
                this.InitializePropertiesFromSchemaContainer();
                this.classEntry = DirectoryEntryManager.GetDirectoryEntry(this.context, (string) this.GetValueFromCache(PropertyManager.DistinguishedName, true));
            }
            return this.classEntry;
        }

        private object GetValueFromCache(string propertyName, bool mustExist)
        {
            object obj2 = null;
            this.InitializePropertiesFromSchemaContainer();
            ArrayList list = (ArrayList) this.propertyValuesFromServer[propertyName.ToLower(CultureInfo.InvariantCulture)];
            if ((list.Count < 1) && mustExist)
            {
                throw new ActiveDirectoryOperationException(Res.GetString("PropertyNotFound", new object[] { propertyName }));
            }
            if (list.Count > 0)
            {
                obj2 = list[0];
            }
            return obj2;
        }

        private ICollection GetValuesFromCache(string propertyName)
        {
            this.InitializePropertiesFromSchemaContainer();
            return (ArrayList) this.propertyValuesFromServer[propertyName.ToLower(CultureInfo.InvariantCulture)];
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
                    this.classEntry = this.schemaEntry.Children.Add(escapedPath, "classSchema");
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
                this.SetProperty(PropertyManager.GovernsID, this.oid);
                this.SetProperty(PropertyManager.Description, this.description);
                if (this.possibleSuperiors != null)
                {
                    this.classEntry.Properties[PropertyManager.PossibleSuperiors].AddRange(this.possibleSuperiors.GetMultiValuedProperty());
                }
                if (this.mandatoryProperties != null)
                {
                    this.classEntry.Properties[PropertyManager.MustContain].AddRange(this.mandatoryProperties.GetMultiValuedProperty());
                }
                if (this.optionalProperties != null)
                {
                    this.classEntry.Properties[PropertyManager.MayContain].AddRange(this.optionalProperties.GetMultiValuedProperty());
                }
                if (this.subClassOf != null)
                {
                    this.SetProperty(PropertyManager.SubClassOf, this.subClassOf.Name);
                }
                else
                {
                    this.SetProperty(PropertyManager.SubClassOf, "top");
                }
                this.SetProperty(PropertyManager.ObjectClassCategory, this.type);
                if (this.schemaGuidBinaryForm != null)
                {
                    this.SetProperty(PropertyManager.SchemaIDGuid, this.schemaGuidBinaryForm);
                }
                if (this.defaultSDSddlForm != null)
                {
                    this.SetProperty(PropertyManager.DefaultSecurityDescriptor, this.defaultSDSddlForm);
                }
            }
            try
            {
                this.classEntry.CommitChanges();
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
            this.description = null;
            this.descriptionInitialized = false;
            this.possibleSuperiors = null;
            this.auxiliaryClasses = null;
            this.possibleInferiors = null;
            this.mandatoryProperties = null;
            this.optionalProperties = null;
            this.subClassOf = null;
            this.typeInitialized = false;
            this.schemaGuidBinaryForm = null;
            this.defaultSDSddlForm = null;
            this.defaultSDSddlFormInitialized = false;
            this.propertiesFromSchemaContainerInitialized = false;
            this.isBound = true;
        }

        private void SetProperty(string propertyName, object value)
        {
            this.GetSchemaClassDirectoryEntry();
            try
            {
                if (value == null)
                {
                    if (this.classEntry.Properties.Contains(propertyName))
                    {
                        this.classEntry.Properties[propertyName].Clear();
                    }
                }
                else
                {
                    this.classEntry.Properties[propertyName].Value = value;
                }
            }
            catch (COMException exception)
            {
                throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public ActiveDirectorySchemaClassCollection AuxiliaryClasses
        {
            get
            {
                this.CheckIfDisposed();
                if (this.auxiliaryClasses == null)
                {
                    if (this.isBound)
                    {
                        if (!this.isDefunctOnServer)
                        {
                            ArrayList classNames = new ArrayList();
                            bool flag = false;
                            object auxDerivedFrom = null;
                            try
                            {
                                auxDerivedFrom = this.iadsClass.AuxDerivedFrom;
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147463155)
                                {
                                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                                }
                                flag = true;
                            }
                            if (!flag)
                            {
                                if (auxDerivedFrom is ICollection)
                                {
                                    classNames.AddRange((ICollection) auxDerivedFrom);
                                }
                                else
                                {
                                    classNames.Add((string) auxDerivedFrom);
                                }
                                this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.AuxiliaryClass, classNames, true);
                            }
                            else
                            {
                                this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.AuxiliaryClass, new ArrayList());
                            }
                        }
                        else
                        {
                            string[] propertyNames = new string[] { PropertyManager.AuxiliaryClass, PropertyManager.SystemAuxiliaryClass };
                            this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.AuxiliaryClass, this.GetClasses(this.GetPropertyValuesRecursively(propertyNames)));
                        }
                    }
                    else
                    {
                        this.auxiliaryClasses = new ActiveDirectorySchemaClassCollection(this.context, this, false, PropertyManager.AuxiliaryClass, new ArrayList());
                    }
                }
                return this.auxiliaryClasses;
            }
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
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.Cn, value);
                }
                this.commonName = value;
            }
        }

        public ActiveDirectorySecurity DefaultObjectSecurityDescriptor
        {
            get
            {
                this.CheckIfDisposed();
                ActiveDirectorySecurity security = null;
                if (this.isBound && !this.defaultSDSddlFormInitialized)
                {
                    this.defaultSDSddlForm = (string) this.GetValueFromCache(PropertyManager.DefaultSecurityDescriptor, false);
                    this.defaultSDSddlFormInitialized = true;
                }
                if (this.defaultSDSddlForm != null)
                {
                    security = new ActiveDirectorySecurity();
                    security.SetSecurityDescriptorSddlForm(this.defaultSDSddlForm);
                }
                return security;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.DefaultSecurityDescriptor, (value == null) ? null : value.GetSecurityDescriptorSddlForm(AccessControlSections.All));
                }
                this.defaultSDSddlForm = (value == null) ? null : value.GetSecurityDescriptorSddlForm(AccessControlSections.All);
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

        public ActiveDirectorySchemaPropertyCollection MandatoryProperties
        {
            get
            {
                this.CheckIfDisposed();
                if (this.mandatoryProperties == null)
                {
                    if (this.isBound)
                    {
                        if (!this.isDefunctOnServer)
                        {
                            ArrayList propertyNames = new ArrayList();
                            bool flag = false;
                            object mandatoryProperties = null;
                            try
                            {
                                mandatoryProperties = this.iadsClass.MandatoryProperties;
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147463155)
                                {
                                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                                }
                                flag = true;
                            }
                            if (!flag)
                            {
                                if (mandatoryProperties is ICollection)
                                {
                                    propertyNames.AddRange((ICollection) mandatoryProperties);
                                }
                                else
                                {
                                    propertyNames.Add((string) mandatoryProperties);
                                }
                                this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MustContain, propertyNames, true);
                            }
                            else
                            {
                                this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MustContain, new ArrayList());
                            }
                        }
                        else
                        {
                            string[] strArray = new string[] { PropertyManager.SystemMustContain, PropertyManager.MustContain };
                            this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MustContain, this.GetProperties(this.GetPropertyValuesRecursively(strArray)));
                        }
                    }
                    else
                    {
                        this.mandatoryProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, false, PropertyManager.MustContain, new ArrayList());
                    }
                }
                return this.mandatoryProperties;
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
                            this.oid = this.iadsClass.OID;
                            goto Label_0056;
                        }
                        catch (COMException exception)
                        {
                            throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                        }
                    }
                    this.oid = (string) this.GetValueFromCache(PropertyManager.GovernsID, true);
                }
            Label_0056:
                return this.oid;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.GovernsID, value);
                }
                this.oid = value;
            }
        }

        public ActiveDirectorySchemaPropertyCollection OptionalProperties
        {
            get
            {
                this.CheckIfDisposed();
                if (this.optionalProperties == null)
                {
                    if (this.isBound)
                    {
                        if (!this.isDefunctOnServer)
                        {
                            ArrayList propertyNames = new ArrayList();
                            bool flag = false;
                            object optionalProperties = null;
                            try
                            {
                                optionalProperties = this.iadsClass.OptionalProperties;
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147463155)
                                {
                                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                                }
                                flag = true;
                            }
                            if (!flag)
                            {
                                if (optionalProperties is ICollection)
                                {
                                    propertyNames.AddRange((ICollection) optionalProperties);
                                }
                                else
                                {
                                    propertyNames.Add((string) optionalProperties);
                                }
                                this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MayContain, propertyNames, true);
                            }
                            else
                            {
                                this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MayContain, new ArrayList());
                            }
                        }
                        else
                        {
                            string[] strArray = new string[] { PropertyManager.SystemMayContain, PropertyManager.MayContain };
                            ArrayList ldapDisplayNames = new ArrayList();
                            foreach (string str in this.GetPropertyValuesRecursively(strArray))
                            {
                                if (!this.MandatoryProperties.Contains(str))
                                {
                                    ldapDisplayNames.Add(str);
                                }
                            }
                            this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, true, PropertyManager.MayContain, this.GetProperties(ldapDisplayNames));
                        }
                    }
                    else
                    {
                        this.optionalProperties = new ActiveDirectorySchemaPropertyCollection(this.context, this, false, PropertyManager.MayContain, new ArrayList());
                    }
                }
                return this.optionalProperties;
            }
        }

        public ReadOnlyActiveDirectorySchemaClassCollection PossibleInferiors
        {
            get
            {
                this.CheckIfDisposed();
                if (this.possibleInferiors == null)
                {
                    if (this.isBound)
                    {
                        this.possibleInferiors = new ReadOnlyActiveDirectorySchemaClassCollection(this.GetClasses(this.GetValuesFromCache(PropertyManager.PossibleInferiors)));
                    }
                    else
                    {
                        this.possibleInferiors = new ReadOnlyActiveDirectorySchemaClassCollection(new ArrayList());
                    }
                }
                return this.possibleInferiors;
            }
        }

        public ActiveDirectorySchemaClassCollection PossibleSuperiors
        {
            get
            {
                this.CheckIfDisposed();
                if (this.possibleSuperiors == null)
                {
                    if (this.isBound)
                    {
                        if (!this.isDefunctOnServer)
                        {
                            ArrayList classNames = new ArrayList();
                            bool flag = false;
                            object possibleSuperiors = null;
                            try
                            {
                                possibleSuperiors = this.iadsClass.PossibleSuperiors;
                            }
                            catch (COMException exception)
                            {
                                if (exception.ErrorCode != -2147463155)
                                {
                                    throw ExceptionHelper.GetExceptionFromCOMException(this.context, exception);
                                }
                                flag = true;
                            }
                            if (!flag)
                            {
                                if (possibleSuperiors is ICollection)
                                {
                                    classNames.AddRange((ICollection) possibleSuperiors);
                                }
                                else
                                {
                                    classNames.Add((string) possibleSuperiors);
                                }
                                this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.PossibleSuperiors, classNames, true);
                            }
                            else
                            {
                                this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.PossibleSuperiors, new ArrayList());
                            }
                        }
                        else
                        {
                            ArrayList ldapDisplayNames = new ArrayList();
                            ldapDisplayNames.AddRange(this.GetValuesFromCache(PropertyManager.PossibleSuperiors));
                            ldapDisplayNames.AddRange(this.GetValuesFromCache(PropertyManager.SystemPossibleSuperiors));
                            this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, true, PropertyManager.PossibleSuperiors, this.GetClasses(ldapDisplayNames));
                        }
                    }
                    else
                    {
                        this.possibleSuperiors = new ActiveDirectorySchemaClassCollection(this.context, this, false, PropertyManager.PossibleSuperiors, new ArrayList());
                    }
                }
                return this.possibleSuperiors;
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

        public ActiveDirectorySchemaClass SubClassOf
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && (this.subClassOf == null))
                {
                    this.subClassOf = new ActiveDirectorySchemaClass(this.context, (string) this.GetValueFromCache(PropertyManager.SubClassOf, true), null, this.schemaEntry);
                }
                return this.subClassOf;
            }
            set
            {
                this.CheckIfDisposed();
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.SubClassOf, value);
                }
                this.subClassOf = value;
            }
        }

        public SchemaClassType Type
        {
            get
            {
                this.CheckIfDisposed();
                if (this.isBound && !this.typeInitialized)
                {
                    this.type = (SchemaClassType) ((int) this.GetValueFromCache(PropertyManager.ObjectClassCategory, true));
                    this.typeInitialized = true;
                }
                return this.type;
            }
            set
            {
                this.CheckIfDisposed();
                if ((value < SchemaClassType.Type88) || (value > SchemaClassType.Auxiliary))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(SchemaClassType));
                }
                if (this.isBound)
                {
                    this.SetProperty(PropertyManager.ObjectClassCategory, value);
                }
                this.type = value;
            }
        }
    }
}

