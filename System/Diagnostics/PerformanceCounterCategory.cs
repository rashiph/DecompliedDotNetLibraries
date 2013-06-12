namespace System.Diagnostics
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Security.Permissions;
    using System.Threading;

    [HostProtection(SecurityAction.LinkDemand, Synchronization=true, SharedState=true)]
    public sealed class PerformanceCounterCategory
    {
        private string categoryHelp;
        private string categoryName;
        private string machineName;
        internal const int MaxCategoryNameLength = 80;
        internal const int MaxCounterNameLength = 0x7fff;
        internal const int MaxHelpLength = 0x7fff;
        private const string perfMutexName = "netfxperf.1.0";

        public PerformanceCounterCategory()
        {
            this.machineName = ".";
        }

        public PerformanceCounterCategory(string categoryName) : this(categoryName, ".")
        {
        }

        public PerformanceCounterCategory(string categoryName, string machineName)
        {
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (categoryName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "categoryName", categoryName }));
            }
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, machineName, categoryName).Demand();
            this.categoryName = categoryName;
            this.machineName = machineName;
        }

        internal static void CheckValidCategory(string categoryName)
        {
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (!CheckValidId(categoryName, 80))
            {
                throw new ArgumentException(SR.GetString("PerfInvalidCategoryName", new object[] { 1, 80 }));
            }
            if (categoryName.Length > (0x400 - "netfxcustomperfcounters.1.0".Length))
            {
                throw new ArgumentException(SR.GetString("CategoryNameTooLong"));
            }
        }

        internal static void CheckValidCounter(string counterName)
        {
            if (counterName == null)
            {
                throw new ArgumentNullException("counterName");
            }
            if (!CheckValidId(counterName, 0x7fff))
            {
                throw new ArgumentException(SR.GetString("PerfInvalidCounterName", new object[] { 1, 0x7fff }));
            }
        }

        internal static void CheckValidCounterLayout(CounterCreationDataCollection counterData)
        {
            Hashtable hashtable = new Hashtable();
            for (int i = 0; i < counterData.Count; i++)
            {
                if ((counterData[i].CounterName == null) || (counterData[i].CounterName.Length == 0))
                {
                    throw new ArgumentException(SR.GetString("InvalidCounterName"));
                }
                int counterType = (int) counterData[i].CounterType;
                switch (counterType)
                {
                    case 0x40020500:
                    case 0x22510500:
                    case 0x23510500:
                    case 0x22410500:
                    case 0x23410500:
                    case 0x20020400:
                    case 0x20c20400:
                    case 0x30020400:
                        if (counterData.Count <= (i + 1))
                        {
                            throw new InvalidOperationException(SR.GetString("CounterLayout"));
                        }
                        if (!PerformanceCounterLib.IsBaseCounter((int) counterData[i + 1].CounterType))
                        {
                            throw new InvalidOperationException(SR.GetString("CounterLayout"));
                        }
                        break;

                    default:
                        if (PerformanceCounterLib.IsBaseCounter(counterType))
                        {
                            if (i == 0)
                            {
                                throw new InvalidOperationException(SR.GetString("CounterLayout"));
                            }
                            counterType = (int) counterData[i - 1].CounterType;
                            if ((((counterType != 0x40020500) && (counterType != 0x22510500)) && ((counterType != 0x23510500) && (counterType != 0x22410500))) && (((counterType != 0x23410500) && (counterType != 0x20020400)) && ((counterType != 0x20c20400) && (counterType != 0x30020400))))
                            {
                                throw new InvalidOperationException(SR.GetString("CounterLayout"));
                            }
                        }
                        break;
                }
                if (hashtable.ContainsKey(counterData[i].CounterName))
                {
                    throw new ArgumentException(SR.GetString("DuplicateCounterName", new object[] { counterData[i].CounterName }));
                }
                hashtable.Add(counterData[i].CounterName, string.Empty);
                if ((counterData[i].CounterHelp == null) || (counterData[i].CounterHelp.Length == 0))
                {
                    counterData[i].CounterHelp = counterData[i].CounterName;
                }
            }
        }

        internal static void CheckValidHelp(string help)
        {
            if (help == null)
            {
                throw new ArgumentNullException("help");
            }
            if (help.Length > 0x7fff)
            {
                throw new ArgumentException(SR.GetString("PerfInvalidHelp", new object[] { 0, 0x7fff }));
            }
        }

        internal static bool CheckValidId(string id, int maxLength)
        {
            if ((id.Length == 0) || (id.Length > maxLength))
            {
                return false;
            }
            for (int i = 0; i < id.Length; i++)
            {
                char c = id[i];
                if (((i == 0) || (i == (id.Length - 1))) && (c == ' '))
                {
                    return false;
                }
                if (c == '"')
                {
                    return false;
                }
                if (char.IsControl(c))
                {
                    return false;
                }
            }
            return true;
        }

        public bool CounterExists(string counterName)
        {
            if (counterName == null)
            {
                throw new ArgumentNullException("counterName");
            }
            if (this.categoryName == null)
            {
                throw new InvalidOperationException(SR.GetString("CategoryNameNotSet"));
            }
            return PerformanceCounterLib.CounterExists(this.machineName, this.categoryName, counterName);
        }

        public static bool CounterExists(string counterName, string categoryName)
        {
            return CounterExists(counterName, categoryName, ".");
        }

        public static bool CounterExists(string counterName, string categoryName, string machineName)
        {
            if (counterName == null)
            {
                throw new ArgumentNullException("counterName");
            }
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (categoryName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "categoryName", categoryName }));
            }
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, machineName, categoryName).Demand();
            return PerformanceCounterLib.CounterExists(machineName, categoryName, counterName);
        }

        [Obsolete("This method has been deprecated.  Please use System.Diagnostics.PerformanceCounterCategory.Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, CounterCreationDataCollection counterData) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, CounterCreationDataCollection counterData)
        {
            return Create(categoryName, categoryHelp, PerformanceCounterCategoryType.Unknown, counterData);
        }

        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, CounterCreationDataCollection counterData)
        {
            PerformanceCounterCategory category;
            if ((categoryType < PerformanceCounterCategoryType.Unknown) || (categoryType > PerformanceCounterCategoryType.MultiInstance))
            {
                throw new ArgumentOutOfRangeException("categoryType");
            }
            if (counterData == null)
            {
                throw new ArgumentNullException("counterData");
            }
            CheckValidCategory(categoryName);
            if (categoryHelp != null)
            {
                CheckValidHelp(categoryHelp);
            }
            string machineName = ".";
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Administer, machineName, categoryName).Demand();
            SharedUtils.CheckNtEnvironment();
            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                SharedUtils.EnterMutex("netfxperf.1.0", ref mutex);
                if (PerformanceCounterLib.IsCustomCategory(machineName, categoryName) || PerformanceCounterLib.CategoryExists(machineName, categoryName))
                {
                    throw new InvalidOperationException(SR.GetString("PerformanceCategoryExists", new object[] { categoryName }));
                }
                CheckValidCounterLayout(counterData);
                PerformanceCounterLib.RegisterCategory(categoryName, categoryType, categoryHelp, counterData);
                category = new PerformanceCounterCategory(categoryName, machineName);
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
            return category;
        }

        [Obsolete("This method has been deprecated.  Please use System.Diagnostics.PerformanceCounterCategory.Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, string counterName, string counterHelp) instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, string counterName, string counterHelp)
        {
            CounterCreationData data = new CounterCreationData(counterName, counterHelp, PerformanceCounterType.NumberOfItems32);
            return Create(categoryName, categoryHelp, PerformanceCounterCategoryType.Unknown, new CounterCreationDataCollection(new CounterCreationData[] { data }));
        }

        public static PerformanceCounterCategory Create(string categoryName, string categoryHelp, PerformanceCounterCategoryType categoryType, string counterName, string counterHelp)
        {
            CounterCreationData data = new CounterCreationData(counterName, counterHelp, PerformanceCounterType.NumberOfItems32);
            return Create(categoryName, categoryHelp, categoryType, new CounterCreationDataCollection(new CounterCreationData[] { data }));
        }

        public static void Delete(string categoryName)
        {
            CheckValidCategory(categoryName);
            string machineName = ".";
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Administer, machineName, categoryName).Demand();
            SharedUtils.CheckNtEnvironment();
            categoryName = categoryName.ToLower(CultureInfo.InvariantCulture);
            Mutex mutex = null;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                SharedUtils.EnterMutex("netfxperf.1.0", ref mutex);
                if (!PerformanceCounterLib.IsCustomCategory(machineName, categoryName))
                {
                    throw new InvalidOperationException(SR.GetString("CantDeleteCategory"));
                }
                SharedPerformanceCounter.RemoveAllInstances(categoryName);
                PerformanceCounterLib.UnregisterCategory(categoryName);
                PerformanceCounterLib.CloseAllLibraries();
            }
            finally
            {
                if (mutex != null)
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                }
            }
        }

        public static bool Exists(string categoryName)
        {
            return Exists(categoryName, ".");
        }

        public static bool Exists(string categoryName, string machineName)
        {
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (categoryName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "categoryName", categoryName }));
            }
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, machineName, categoryName).Demand();
            return (PerformanceCounterLib.IsCustomCategory(machineName, categoryName) || PerformanceCounterLib.CategoryExists(machineName, categoryName));
        }

        public static PerformanceCounterCategory[] GetCategories()
        {
            return GetCategories(".");
        }

        public static PerformanceCounterCategory[] GetCategories(string machineName)
        {
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, machineName, "*").Demand();
            string[] categories = PerformanceCounterLib.GetCategories(machineName);
            PerformanceCounterCategory[] categoryArray = new PerformanceCounterCategory[categories.Length];
            for (int i = 0; i < categoryArray.Length; i++)
            {
                categoryArray[i] = new PerformanceCounterCategory(categories[i], machineName);
            }
            return categoryArray;
        }

        internal static string[] GetCounterInstances(string categoryName, string machineName)
        {
            new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, machineName, categoryName).Demand();
            CategorySample categorySample = PerformanceCounterLib.GetCategorySample(machineName, categoryName);
            if (categorySample.InstanceNameTable.Count == 0)
            {
                return new string[0];
            }
            string[] array = new string[categorySample.InstanceNameTable.Count];
            categorySample.InstanceNameTable.Keys.CopyTo(array, 0);
            if ((array.Length == 1) && (array[0].CompareTo("systemdiagnosticsperfcounterlibsingleinstance") == 0))
            {
                return new string[0];
            }
            return array;
        }

        public PerformanceCounter[] GetCounters()
        {
            if (this.GetInstanceNames().Length != 0)
            {
                throw new ArgumentException(SR.GetString("InstanceNameRequired"));
            }
            return this.GetCounters("");
        }

        public PerformanceCounter[] GetCounters(string instanceName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }
            if (this.categoryName == null)
            {
                throw new InvalidOperationException(SR.GetString("CategoryNameNotSet"));
            }
            if ((instanceName.Length != 0) && !this.InstanceExists(instanceName))
            {
                throw new InvalidOperationException(SR.GetString("MissingInstance", new object[] { instanceName, this.categoryName }));
            }
            string[] counters = PerformanceCounterLib.GetCounters(this.machineName, this.categoryName);
            PerformanceCounter[] counterArray = new PerformanceCounter[counters.Length];
            for (int i = 0; i < counterArray.Length; i++)
            {
                counterArray[i] = new PerformanceCounter(this.categoryName, counters[i], instanceName, this.machineName, true);
            }
            return counterArray;
        }

        public string[] GetInstanceNames()
        {
            if (this.categoryName == null)
            {
                throw new InvalidOperationException(SR.GetString("CategoryNameNotSet"));
            }
            return GetCounterInstances(this.categoryName, this.machineName);
        }

        public bool InstanceExists(string instanceName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }
            if (this.categoryName == null)
            {
                throw new InvalidOperationException(SR.GetString("CategoryNameNotSet"));
            }
            return PerformanceCounterLib.GetCategorySample(this.machineName, this.categoryName).InstanceNameTable.ContainsKey(instanceName);
        }

        public static bool InstanceExists(string instanceName, string categoryName)
        {
            return InstanceExists(instanceName, categoryName, ".");
        }

        public static bool InstanceExists(string instanceName, string categoryName, string machineName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException("instanceName");
            }
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (categoryName.Length == 0)
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "categoryName", categoryName }));
            }
            if (!SyntaxCheck.CheckMachineName(machineName))
            {
                throw new ArgumentException(SR.GetString("InvalidParameter", new object[] { "machineName", machineName }));
            }
            PerformanceCounterCategory category = new PerformanceCounterCategory(categoryName, machineName);
            return category.InstanceExists(instanceName);
        }

        public InstanceDataCollectionCollection ReadCategory()
        {
            if (this.categoryName == null)
            {
                throw new InvalidOperationException(SR.GetString("CategoryNameNotSet"));
            }
            return PerformanceCounterLib.GetCategorySample(this.machineName, this.categoryName).ReadCategory();
        }

        public string CategoryHelp
        {
            get
            {
                if (this.categoryName == null)
                {
                    throw new InvalidOperationException(SR.GetString("CategoryNameNotSet"));
                }
                if (this.categoryHelp == null)
                {
                    this.categoryHelp = PerformanceCounterLib.GetCategoryHelp(this.machineName, this.categoryName);
                }
                return this.categoryHelp;
            }
        }

        public string CategoryName
        {
            get
            {
                return this.categoryName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (value.Length == 0)
                {
                    throw new ArgumentException(SR.GetString("InvalidProperty", new object[] { "CategoryName", value }));
                }
                lock (this)
                {
                    new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, this.machineName, value).Demand();
                    this.categoryName = value;
                }
            }
        }

        public PerformanceCounterCategoryType CategoryType
        {
            get
            {
                if (PerformanceCounterLib.GetCategorySample(this.machineName, this.categoryName).IsMultiInstance)
                {
                    return PerformanceCounterCategoryType.MultiInstance;
                }
                if (PerformanceCounterLib.IsCustomCategory(".", this.categoryName))
                {
                    return PerformanceCounterLib.GetCategoryType(".", this.categoryName);
                }
                return PerformanceCounterCategoryType.SingleInstance;
            }
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
            set
            {
                if (!SyntaxCheck.CheckMachineName(value))
                {
                    throw new ArgumentException(SR.GetString("InvalidProperty", new object[] { "MachineName", value }));
                }
                lock (this)
                {
                    if (this.categoryName != null)
                    {
                        new PerformanceCounterPermission(PerformanceCounterPermissionAccess.Browse, value, this.categoryName).Demand();
                    }
                    this.machineName = value;
                }
            }
        }
    }
}

