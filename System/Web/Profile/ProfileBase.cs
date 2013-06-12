namespace System.Web.Profile
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.Hosting;
    using System.Web.Security;

    public class ProfileBase : SettingsBase
    {
        private bool _DatesRetrieved;
        private Hashtable _Groups = new Hashtable();
        private bool _IsAuthenticated;
        private bool _IsDirty;
        private DateTime _LastActivityDate;
        private DateTime _LastUpdatedDate;
        private string _UserName;
        private static bool s_Initialized = false;
        private static Exception s_InitializeException = null;
        private static object s_InitializeLock = new object();
        private static SettingsPropertyCollection s_Properties = null;
        private static Hashtable s_PropertiesForCompilation = null;
        private static ProfileBase s_SingletonInstance = null;

        public ProfileBase()
        {
            if (!ProfileManager.Enabled)
            {
                throw new ProviderException(System.Web.SR.GetString("Profile_not_enabled"));
            }
            if (!s_Initialized)
            {
                InitializeStatic();
            }
        }

        private static void AddProfilePropertySettingsForCompilation(ProfilePropertySettingsCollection propertyCollection, Hashtable ht, string groupName)
        {
            foreach (ProfilePropertySettings settings in propertyCollection)
            {
                ProfileNameTypeStruct struct2 = new ProfileNameTypeStruct();
                if (groupName != null)
                {
                    struct2.Name = groupName + "." + settings.Name;
                }
                else
                {
                    struct2.Name = settings.Name;
                }
                Type typeInternal = settings.TypeInternal;
                if (typeInternal == null)
                {
                    typeInternal = ResolvePropertyTypeForCommonTypes(settings.Type.ToLower(CultureInfo.InvariantCulture));
                }
                if (typeInternal == null)
                {
                    typeInternal = BuildManager.GetType(settings.Type, false);
                }
                if (typeInternal == null)
                {
                    struct2.PropertyCodeRefType = new CodeTypeReference(settings.Type);
                }
                else
                {
                    struct2.PropertyCodeRefType = new CodeTypeReference(typeInternal);
                }
                struct2.PropertyType = typeInternal;
                settings.TypeInternal = typeInternal;
                struct2.IsReadOnly = settings.ReadOnly;
                struct2.LineNumber = settings.ElementInformation.Properties["name"].LineNumber;
                struct2.FileName = settings.ElementInformation.Properties["name"].Source;
                ht.Add(struct2.Name, struct2);
            }
        }

        private static void AddPropertySettingsFromConfig(Type baseType, bool fAnonEnabled, bool hasLowTrust, ProfilePropertySettingsCollection settingsCollection, string groupName)
        {
            foreach (ProfilePropertySettings settings in settingsCollection)
            {
                string name = (groupName != null) ? (groupName + "." + settings.Name) : settings.Name;
                if ((baseType != typeof(ProfileBase)) && (s_Properties[name] != null))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_property_already_added"), null, settings.ElementInformation.Properties["name"].Source, settings.ElementInformation.Properties["name"].LineNumber);
                }
                try
                {
                    if (settings.TypeInternal == null)
                    {
                        settings.TypeInternal = ResolvePropertyType(settings.Type);
                    }
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_could_not_create_type", new object[] { exception.Message }), exception, settings.ElementInformation.Properties["type"].Source, settings.ElementInformation.Properties["type"].LineNumber);
                }
                if (!fAnonEnabled && settings.AllowAnonymous)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Annoymous_id_module_not_enabled", new object[] { settings.Name }), settings.ElementInformation.Properties["allowAnonymous"].Source, settings.ElementInformation.Properties["allowAnonymous"].LineNumber);
                }
                if ((settings.SerializeAs == SerializationMode.Binary) && !settings.TypeInternal.IsSerializable)
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Property_not_serializable", new object[] { settings.Name }), settings.ElementInformation.Properties["serializeAs"].Source, settings.ElementInformation.Properties["serializeAs"].LineNumber);
                }
                if (hasLowTrust)
                {
                    SetProviderForProperty(settings);
                }
                else
                {
                    settings.ProviderInternal = null;
                }
                SettingsAttributeDictionary attributes = new SettingsAttributeDictionary();
                attributes.Add("AllowAnonymous", settings.AllowAnonymous);
                if (!string.IsNullOrEmpty(settings.CustomProviderData))
                {
                    attributes.Add("CustomProviderData", settings.CustomProviderData);
                }
                SettingsProperty property = new SettingsProperty(name, settings.TypeInternal, settings.ProviderInternal, settings.ReadOnly, settings.DefaultValue, (SettingsSerializeAs) settings.SerializeAs, attributes, false, true);
                s_Properties.Add(property);
            }
        }

        public static ProfileBase Create(string username)
        {
            return Create(username, true);
        }

        public static ProfileBase Create(string username, bool isAuthenticated)
        {
            if (!ProfileManager.Enabled)
            {
                throw new ProviderException(System.Web.SR.GetString("Profile_not_enabled"));
            }
            InitializeStatic();
            if (s_SingletonInstance != null)
            {
                return s_SingletonInstance;
            }
            if (s_Properties.Count == 0)
            {
                lock (s_InitializeLock)
                {
                    if (s_SingletonInstance == null)
                    {
                        s_SingletonInstance = new DefaultProfile();
                    }
                    return s_SingletonInstance;
                }
            }
            HttpRuntime.CheckAspNetHostingPermission(AspNetHostingPermissionLevel.Low, "Feature_not_supported_at_this_level");
            return CreateMyInstance(username, isAuthenticated);
        }

        private static ProfileBase CreateMyInstance(string username, bool isAuthenticated)
        {
            Type profileType;
            if (HostingEnvironment.IsHosted)
            {
                profileType = BuildManager.GetProfileType();
            }
            else
            {
                profileType = InheritsFromType;
            }
            ProfileBase base2 = (ProfileBase) Activator.CreateInstance(profileType);
            base2.Initialize(username, isAuthenticated);
            return base2;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private object GetInternal(string propertyName)
        {
            return base[propertyName];
        }

        internal static string GetProfileClassName()
        {
            Hashtable propertiesForCompilation = GetPropertiesForCompilation();
            if (propertiesForCompilation == null)
            {
                return "System.Web.Profile.DefaultProfile";
            }
            if ((propertiesForCompilation.Count <= 0) && !InheritsFromCustomType)
            {
                return "System.Web.Profile.DefaultProfile";
            }
            return "ProfileCommon";
        }

        public ProfileGroupBase GetProfileGroup(string groupName)
        {
            ProfileGroupBase base2 = (ProfileGroupBase) this._Groups[groupName];
            if (base2 == null)
            {
                Type profileType = BuildManager.GetProfileType();
                if (profileType == null)
                {
                    throw new ProviderException(System.Web.SR.GetString("Profile_group_not_found", new object[] { groupName }));
                }
                profileType = profileType.Assembly.GetType("ProfileGroup" + groupName, false);
                if (profileType == null)
                {
                    throw new ProviderException(System.Web.SR.GetString("Profile_group_not_found", new object[] { groupName }));
                }
                base2 = (ProfileGroupBase) Activator.CreateInstance(profileType);
                base2.Init(this, groupName);
            }
            return base2;
        }

        internal static Hashtable GetPropertiesForCompilation()
        {
            if (!ProfileManager.Enabled)
            {
                return null;
            }
            if (s_PropertiesForCompilation == null)
            {
                lock (s_InitializeLock)
                {
                    if (s_PropertiesForCompilation != null)
                    {
                        return s_PropertiesForCompilation;
                    }
                    Hashtable ht = new Hashtable();
                    ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                    if (profileAppConfig.PropertySettings == null)
                    {
                        s_PropertiesForCompilation = ht;
                        return s_PropertiesForCompilation;
                    }
                    AddProfilePropertySettingsForCompilation(profileAppConfig.PropertySettings, ht, null);
                    foreach (ProfileGroupSettings settings in profileAppConfig.PropertySettings.GroupSettings)
                    {
                        AddProfilePropertySettingsForCompilation(settings.PropertySettings, ht, settings.Name);
                    }
                    AddProfilePropertySettingsForCompilation(ProfileManager.DynamicProfileProperties, ht, null);
                    s_PropertiesForCompilation = ht;
                }
            }
            return s_PropertiesForCompilation;
        }

        public object GetPropertyValue(string propertyName)
        {
            return this[propertyName];
        }

        private static Type GetPropType(string typeName)
        {
            Exception exception = null;
            try
            {
                return Type.GetType(typeName, true, true);
            }
            catch (Exception exception2)
            {
                exception = exception2;
            }
            try
            {
                CompilationSection compilationAppConfig = MTConfigUtil.GetCompilationAppConfig();
                if (compilationAppConfig != null)
                {
                    AssemblyCollection assemblies = compilationAppConfig.Assemblies;
                    if (assemblies != null)
                    {
                        foreach (Assembly assembly in assemblies)
                        {
                            Type type = assembly.GetType(typeName, false, true);
                            if (type != null)
                            {
                                return type;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
            throw exception;
        }

        public void Initialize(string username, bool isAuthenticated)
        {
            if (username != null)
            {
                this._UserName = username.Trim();
            }
            else
            {
                this._UserName = username;
            }
            SettingsContext context = new SettingsContext();
            context.Add("UserName", this._UserName);
            context.Add("IsAuthenticated", isAuthenticated);
            this._IsAuthenticated = isAuthenticated;
            base.Initialize(context, s_Properties, ProfileManager.Providers);
        }

        private static void InitializeStatic()
        {
            if (!ProfileManager.Enabled || s_Initialized)
            {
                if (s_InitializeException != null)
                {
                    throw s_InitializeException;
                }
            }
            else
            {
                lock (s_InitializeLock)
                {
                    if (s_Initialized)
                    {
                        if (s_InitializeException != null)
                        {
                            throw s_InitializeException;
                        }
                        return;
                    }
                    try
                    {
                        ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                        bool fAnonEnabled = HostingEnvironment.IsHosted ? AnonymousIdentificationModule.Enabled : true;
                        Type inheritsFromType = InheritsFromType;
                        bool hasLowTrust = HttpRuntime.HasAspNetHostingPermission(AspNetHostingPermissionLevel.Low);
                        s_Properties = new SettingsPropertyCollection();
                        AddPropertySettingsFromConfig(inheritsFromType, fAnonEnabled, hasLowTrust, ProfileManager.DynamicProfileProperties, null);
                        if (inheritsFromType != typeof(ProfileBase))
                        {
                            PropertyInfo[] properties = typeof(ProfileBase).GetProperties();
                            NameValueCollection values = new NameValueCollection(properties.Length);
                            foreach (PropertyInfo info in properties)
                            {
                                values.Add(info.Name, string.Empty);
                            }
                            foreach (PropertyInfo info2 in inheritsFromType.GetProperties())
                            {
                                if (values[info2.Name] == null)
                                {
                                    ProfileProvider provider = hasLowTrust ? ProfileManager.Provider : null;
                                    bool isReadOnly = false;
                                    SettingsSerializeAs providerSpecific = SettingsSerializeAs.ProviderSpecific;
                                    string defaultValue = string.Empty;
                                    bool allow = false;
                                    string customProviderData = null;
                                    foreach (Attribute attribute in Attribute.GetCustomAttributes(info2, true))
                                    {
                                        if (attribute is SettingsSerializeAsAttribute)
                                        {
                                            providerSpecific = ((SettingsSerializeAsAttribute) attribute).SerializeAs;
                                        }
                                        else if (attribute is SettingsAllowAnonymousAttribute)
                                        {
                                            allow = ((SettingsAllowAnonymousAttribute) attribute).Allow;
                                            if (!fAnonEnabled && allow)
                                            {
                                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Annoymous_id_module_not_enabled", new object[] { info2.Name }), profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherits"].LineNumber);
                                            }
                                        }
                                        else if (attribute is ReadOnlyAttribute)
                                        {
                                            isReadOnly = ((ReadOnlyAttribute) attribute).IsReadOnly;
                                        }
                                        else if (attribute is DefaultSettingValueAttribute)
                                        {
                                            defaultValue = ((DefaultSettingValueAttribute) attribute).Value;
                                        }
                                        else if (attribute is CustomProviderDataAttribute)
                                        {
                                            customProviderData = ((CustomProviderDataAttribute) attribute).CustomProviderData;
                                        }
                                        else if (hasLowTrust && (attribute is ProfileProviderAttribute))
                                        {
                                            provider = ProfileManager.Providers[((ProfileProviderAttribute) attribute).ProviderName];
                                            if (provider == null)
                                            {
                                                throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_provider_not_found", new object[] { ((ProfileProviderAttribute) attribute).ProviderName }), profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherits"].LineNumber);
                                            }
                                        }
                                    }
                                    SettingsAttributeDictionary attributes = new SettingsAttributeDictionary();
                                    attributes.Add("AllowAnonymous", allow);
                                    if (!string.IsNullOrEmpty(customProviderData))
                                    {
                                        attributes.Add("CustomProviderData", customProviderData);
                                    }
                                    SettingsProperty property = new SettingsProperty(info2.Name, info2.PropertyType, provider, isReadOnly, defaultValue, providerSpecific, attributes, false, true);
                                    s_Properties.Add(property);
                                }
                            }
                        }
                        if (profileAppConfig.PropertySettings != null)
                        {
                            AddPropertySettingsFromConfig(inheritsFromType, fAnonEnabled, hasLowTrust, profileAppConfig.PropertySettings, null);
                            foreach (ProfileGroupSettings settings in profileAppConfig.PropertySettings.GroupSettings)
                            {
                                AddPropertySettingsFromConfig(inheritsFromType, fAnonEnabled, hasLowTrust, settings.PropertySettings, settings.Name);
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (s_InitializeException == null)
                        {
                            s_InitializeException = exception;
                        }
                    }
                    if (s_Properties == null)
                    {
                        s_Properties = new SettingsPropertyCollection();
                    }
                    s_Properties.SetReadOnly();
                    s_Initialized = true;
                }
                if (s_InitializeException != null)
                {
                    throw s_InitializeException;
                }
            }
        }

        private static Type ResolvePropertyType(string typeName)
        {
            Type type = ResolvePropertyTypeForCommonTypes(typeName.ToLower(CultureInfo.InvariantCulture));
            if (type != null)
            {
                return type;
            }
            if (HostingEnvironment.IsHosted)
            {
                return BuildManager.GetType(typeName, true, true);
            }
            return GetPropType(typeName);
        }

        private static Type ResolvePropertyTypeForCommonTypes(string typeName)
        {
            switch (typeName)
            {
                case "string":
                    return typeof(string);

                case "byte":
                case "int8":
                    return typeof(byte);

                case "boolean":
                case "bool":
                    return typeof(bool);

                case "char":
                    return typeof(char);

                case "int":
                case "integer":
                case "int32":
                    return typeof(int);

                case "date":
                case "datetime":
                    return typeof(DateTime);

                case "decimal":
                    return typeof(decimal);

                case "double":
                case "float64":
                    return typeof(double);

                case "float":
                case "float32":
                    return typeof(float);

                case "long":
                case "int64":
                    return typeof(long);

                case "short":
                case "int16":
                    return typeof(short);

                case "single":
                    return typeof(float);

                case "uint16":
                case "ushort":
                    return typeof(ushort);

                case "uint32":
                case "uint":
                    return typeof(uint);

                case "ulong":
                case "uint64":
                    return typeof(ulong);

                case "object":
                    return typeof(object);
            }
            return null;
        }

        private void RetrieveDates()
        {
            if (!this._DatesRetrieved && (ProfileManager.Provider != null))
            {
                int num;
                foreach (ProfileInfo info in ProfileManager.Provider.FindProfilesByUserName(ProfileAuthenticationOption.All, this._UserName, 0, 1, out num))
                {
                    this._LastActivityDate = info.LastActivityDate.ToUniversalTime();
                    this._LastUpdatedDate = info.LastUpdatedDate.ToUniversalTime();
                    this._DatesRetrieved = true;
                    break;
                }
            }
        }

        public override void Save()
        {
            if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
            {
                HttpRuntime.NamedPermissionSet.PermitOnly();
            }
            this.SaveWithAssert();
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private void SaveWithAssert()
        {
            base.Save();
            this._IsDirty = false;
            this._DatesRetrieved = false;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.SerializationFormatter)]
        private void SetInternal(string propertyName, object value)
        {
            if (!this._IsAuthenticated)
            {
                SettingsProperty property = s_Properties[propertyName];
                if ((property != null) && !((bool) property.Attributes["AllowAnonymous"]))
                {
                    throw new ProviderException(System.Web.SR.GetString("Profile_anonoymous_not_allowed_to_set_property"));
                }
            }
            base[propertyName] = value;
        }

        public void SetPropertyValue(string propertyName, object propertyValue)
        {
            this[propertyName] = propertyValue;
        }

        private static void SetProviderForProperty(ProfilePropertySettings pps)
        {
            if ((pps.Provider == null) || (pps.Provider.Length < 1))
            {
                pps.ProviderInternal = ProfileManager.Provider;
            }
            else
            {
                pps.ProviderInternal = ProfileManager.Providers[pps.Provider];
            }
            if (pps.ProviderInternal == null)
            {
                throw new ConfigurationErrorsException(System.Web.SR.GetString("Profile_provider_not_found", new object[] { pps.Provider }), pps.ElementInformation.Properties["provider"].Source, pps.ElementInformation.Properties["provider"].LineNumber);
            }
        }

        internal static bool InheritsFromCustomType
        {
            get
            {
                if (!ProfileManager.Enabled)
                {
                    return false;
                }
                ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                if (profileAppConfig.Inherits == null)
                {
                    return false;
                }
                string typeName = profileAppConfig.Inherits.Trim();
                if ((typeName == null) || (typeName.Length < 1))
                {
                    return false;
                }
                Type type = Type.GetType(typeName, false, true);
                if ((type != null) && !(type != typeof(ProfileBase)))
                {
                    return false;
                }
                return true;
            }
        }

        internal static Type InheritsFromType
        {
            get
            {
                Type propType;
                if (!ProfileManager.Enabled)
                {
                    return typeof(DefaultProfile);
                }
                if (HostingEnvironment.IsHosted)
                {
                    propType = BuildManager.GetType(InheritsFromTypeString, true, true);
                }
                else
                {
                    propType = GetPropType(InheritsFromTypeString);
                }
                if (!typeof(ProfileBase).IsAssignableFrom(propType))
                {
                    ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_profile_base_type"), null, profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherit"].LineNumber);
                }
                return propType;
            }
        }

        internal static string InheritsFromTypeString
        {
            get
            {
                string str = typeof(ProfileBase).ToString();
                if (!ProfileManager.Enabled)
                {
                    return str;
                }
                ProfileSection profileAppConfig = MTConfigUtil.GetProfileAppConfig();
                if (profileAppConfig.Inherits == null)
                {
                    return str;
                }
                string typeName = profileAppConfig.Inherits.Trim();
                if (typeName.Length < 1)
                {
                    return str;
                }
                Type c = Type.GetType(typeName, false, true);
                if (c == null)
                {
                    return typeName;
                }
                if (!typeof(ProfileBase).IsAssignableFrom(c))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Wrong_profile_base_type"), null, profileAppConfig.ElementInformation.Properties["inherits"].Source, profileAppConfig.ElementInformation.Properties["inherit"].LineNumber);
                }
                return c.AssemblyQualifiedName;
            }
        }

        public bool IsAnonymous
        {
            get
            {
                return !this._IsAuthenticated;
            }
        }

        public bool IsDirty
        {
            get
            {
                if (this._IsDirty)
                {
                    return true;
                }
                foreach (SettingsPropertyValue value2 in this.PropertyValues)
                {
                    if (value2.IsDirty)
                    {
                        this._IsDirty = true;
                        return true;
                    }
                }
                return false;
            }
        }

        public override object this[string propertyName]
        {
            get
            {
                if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
                {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
                return this.GetInternal(propertyName);
            }
            set
            {
                if ((!HttpRuntime.DisableProcessRequestInApplicationTrust && (HttpRuntime.NamedPermissionSet != null)) && HttpRuntime.ProcessRequestInApplicationTrust)
                {
                    HttpRuntime.NamedPermissionSet.PermitOnly();
                }
                this.SetInternal(propertyName, value);
            }
        }

        public DateTime LastActivityDate
        {
            get
            {
                if (!this._DatesRetrieved)
                {
                    this.RetrieveDates();
                }
                return this._LastActivityDate.ToLocalTime();
            }
        }

        public DateTime LastUpdatedDate
        {
            get
            {
                if (!this._DatesRetrieved)
                {
                    this.RetrieveDates();
                }
                return this._LastUpdatedDate.ToLocalTime();
            }
        }

        public static SettingsPropertyCollection Properties
        {
            get
            {
                InitializeStatic();
                return s_Properties;
            }
        }

        internal static ProfileBase SingletonInstance
        {
            get
            {
                return s_SingletonInstance;
            }
        }

        public string UserName
        {
            get
            {
                return this._UserName;
            }
        }
    }
}

