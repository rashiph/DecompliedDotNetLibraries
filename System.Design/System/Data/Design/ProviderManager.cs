namespace System.Data.Design
{
    using System;
    using System.Collections;
    using System.Data;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    internal sealed class ProviderManager
    {
        internal static DbProviderFactory ActiveFactoryContext = null;
        internal static Hashtable CustomDBProviders = null;
        private static DataTable factoryTable = null;
        private static readonly string PROVIDER_ASSEMBLY = "AssemblyQualifiedName";
        private static readonly string PROVIDER_INVARIANT_NAME = "InvariantName";
        private static readonly string PROVIDER_NAME = "Name";
        private static CachedProviderData providerData = new CachedProviderData();

        private static object CreateObject(DbProviderFactory factory, ProviderSupportedClasses kindOfObject, string providerName)
        {
            switch (kindOfObject)
            {
                case ProviderSupportedClasses.DbConnection:
                    return factory.CreateConnection();

                case ProviderSupportedClasses.DbDataAdapter:
                    return factory.CreateDataAdapter();

                case ProviderSupportedClasses.DbParameter:
                    return factory.CreateParameter();

                case ProviderSupportedClasses.DbCommand:
                    return factory.CreateCommand();

                case ProviderSupportedClasses.DbCommandBuilder:
                    return factory.CreateCommandBuilder();

                case ProviderSupportedClasses.DbDataSourceEnumerator:
                    return factory.CreateDataSourceEnumerator();

                case ProviderSupportedClasses.CodeAccessPermission:
                    return factory.CreatePermission(PermissionState.None);
            }
            throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Cannot create object of provider class identified by enum {0} for provider {1}", new object[] { Enum.GetName(typeof(ProviderSupportedClasses), kindOfObject), providerName }));
        }

        private static void EnsureFactoryTable()
        {
            if (factoryTable == null)
            {
                factoryTable = DbProviderFactories.GetFactoryClasses();
                if (factoryTable == null)
                {
                    throw new InternalException("Unable to get factory-table.");
                }
            }
        }

        public static DbProviderFactory GetFactory(string invariantName)
        {
            if (StringUtil.EmptyOrSpace(invariantName))
            {
                throw new ArgumentNullException("invariantName");
            }
            if (ActiveFactoryContext != null)
            {
                providerData.Initialize(ActiveFactoryContext, invariantName, invariantName);
                return ActiveFactoryContext;
            }
            if ((CustomDBProviders != null) && CustomDBProviders.ContainsKey(invariantName))
            {
                DbProviderFactory factory = CustomDBProviders[invariantName] as DbProviderFactory;
                if (factory != null)
                {
                    providerData.Initialize(factory, invariantName, invariantName);
                    return factory;
                }
            }
            if (providerData.Matches(invariantName))
            {
                return providerData.CachedFactory;
            }
            EnsureFactoryTable();
            DataRow[] rowArray = factoryTable.Select(string.Format(CultureInfo.CurrentCulture, "InvariantName = '{0}'", new object[] { invariantName }));
            if (rowArray.Length == 0)
            {
                throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Cannot find provider factory for provider named {0}", new object[] { invariantName }));
            }
            if (rowArray.Length > 1)
            {
                throw new InternalException(string.Format(CultureInfo.CurrentCulture, "More that one data row for provider named {0}", new object[] { invariantName }));
            }
            DbProviderFactory factory2 = DbProviderFactories.GetFactory(rowArray[0]);
            providerData.Initialize(factory2, invariantName, (string) rowArray[0][PROVIDER_NAME]);
            return factory2;
        }

        public static DbProviderFactory GetFactoryFromType(Type type, ProviderSupportedClasses kindOfObject)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (providerData.Matches(type))
            {
                return providerData.CachedFactory;
            }
            EnsureFactoryTable();
            foreach (DataRow row in factoryTable.Rows)
            {
                DbProviderFactory factory = DbProviderFactories.GetFactory(row);
                string providerName = (string) row[PROVIDER_NAME];
                object obj2 = CreateObject(factory, kindOfObject, providerName);
                if (type.Equals(obj2.GetType()))
                {
                    providerData.Initialize(factory, (string) row[PROVIDER_INVARIANT_NAME], (string) row[PROVIDER_NAME], type);
                    return factory;
                }
            }
            throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Unable to find DbProviderFactory for type {0}", new object[] { type.ToString() }));
        }

        public static string GetInvariantProviderName(DbProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory");
            }
            if (providerData.Matches(factory))
            {
                return providerData.CachedInvariantProviderName;
            }
            EnsureFactoryTable();
            string assemblyQualifiedName = factory.GetType().AssemblyQualifiedName;
            foreach (DataRow row in factoryTable.Rows)
            {
                if (StringUtil.EqualValue((string) row[PROVIDER_ASSEMBLY], assemblyQualifiedName))
                {
                    providerData.Initialize(factory, (string) row[PROVIDER_INVARIANT_NAME], (string) row[PROVIDER_NAME]);
                    return providerData.CachedInvariantProviderName;
                }
            }
            throw new InternalException(string.Format(CultureInfo.CurrentCulture, "Unable to get invariant name from factory. Factory type is {0}", new object[] { factory.GetType().ToString() }));
        }

        public static PropertyInfo GetProviderTypeProperty(DbProviderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException("factory should not be null.");
            }
            if (providerData.UseCachedPropertyValue)
            {
                return providerData.ProviderTypeProperty;
            }
            providerData.UseCachedPropertyValue = true;
            foreach (PropertyInfo info in factory.CreateParameter().GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (info.PropertyType.IsEnum)
                {
                    object[] customAttributes = info.GetCustomAttributes(typeof(DbProviderSpecificTypePropertyAttribute), true);
                    if ((customAttributes.Length > 0) && ((DbProviderSpecificTypePropertyAttribute) customAttributes[0]).IsProviderSpecificTypeProperty)
                    {
                        providerData.ProviderTypeProperty = info;
                        return info;
                    }
                }
            }
            providerData.ProviderTypeProperty = null;
            return null;
        }

        private class CachedProviderData
        {
            public string CachedDisplayName = string.Empty;
            public DbProviderFactory CachedFactory;
            public string CachedInvariantProviderName = string.Empty;
            public Type CachedType;
            private PropertyInfo providerTypeProperty;
            private bool useCachedPropertyValue;

            public void Initialize(DbProviderFactory factory, string invariantProviderName, string displayName)
            {
                this.CachedFactory = factory;
                this.CachedInvariantProviderName = invariantProviderName;
                this.CachedType = null;
                this.CachedDisplayName = displayName;
                this.ProviderTypeProperty = null;
                this.UseCachedPropertyValue = false;
            }

            public void Initialize(DbProviderFactory factory, string invariantProviderName, string displayName, Type type)
            {
                this.Initialize(factory, invariantProviderName, displayName);
                this.CachedType = type;
            }

            public bool Matches(DbProviderFactory factory)
            {
                return ((this.CachedFactory != null) && this.CachedFactory.GetType().Equals(factory.GetType()));
            }

            public bool Matches(string invariantName)
            {
                return (((this.CachedFactory != null) && (this.CachedInvariantProviderName != null)) && StringUtil.EqualValue(this.CachedInvariantProviderName, invariantName));
            }

            public bool Matches(Type type)
            {
                return (((this.CachedFactory != null) && (this.CachedType != null)) && this.CachedType.Equals(type));
            }

            public PropertyInfo ProviderTypeProperty
            {
                get
                {
                    return this.providerTypeProperty;
                }
                set
                {
                    this.providerTypeProperty = value;
                }
            }

            public bool UseCachedPropertyValue
            {
                get
                {
                    return this.useCachedPropertyValue;
                }
                set
                {
                    this.useCachedPropertyValue = value;
                }
            }
        }

        internal enum ProviderSupportedClasses
        {
            DbConnection,
            DbDataAdapter,
            DbParameter,
            DbCommand,
            DbCommandBuilder,
            DbDataSourceEnumerator,
            CodeAccessPermission,
            DbConnectionStringBuilder
        }
    }
}

