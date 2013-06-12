namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;

    internal class PerformanceCounterLib
    {
        private const string categorySymbolPrefix = "OBJECT_";
        private Hashtable categoryTable;
        private readonly object CategoryTableLock = new object();
        internal const string CloseEntryPoint = "ClosePerformanceData";
        internal const string CollectEntryPoint = "CollectPerformanceData";
        private static string computerName;
        private const string conterSymbolPrefix = "DEVICE_COUNTER_";
        private Hashtable customCategoryTable;
        private const string defineKeyword = "#define";
        private const string DllName = "netfxperf.dll";
        private const string driverNameKeyword = "drivername";
        private const int EnglishLCID = 9;
        private const string helpSufix = "_HELP";
        private Hashtable helpTable;
        private readonly object HelpTableLock = new object();
        private const string infoDefinition = "[info]";
        private static string iniFilePath;
        private const string languageDefinition = "[languages]";
        private const string languageKeyword = "language";
        private static Hashtable libraryTable;
        private string machineName;
        private const string nameSufix = "_NAME";
        private Hashtable nameTable;
        private readonly object NameTableLock = new object();
        private const string objectDefinition = "[objects]";
        internal const string OpenEntryPoint = "OpenPerformanceData";
        private string perfLcid;
        private const string PerflibPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Perflib";
        private PerformanceMonitor performanceMonitor;
        internal const string PerfShimName = "netfxperf.dll";
        private static object s_InternalSyncObject;
        internal const string ServicePath = @"SYSTEM\CurrentControlSet\Services";
        internal const string SingleInstanceName = "systemdiagnosticsperfcounterlibsingleinstance";
        private const string symbolFileKeyword = "symbolfile";
        private static string symbolFilePath;
        private const string textDefinition = "[text]";

        internal PerformanceCounterLib(string machineName, string lcid)
        {
            this.machineName = machineName;
            this.perfLcid = lcid;
        }

        internal bool CategoryExists(string category)
        {
            return this.CategoryTable.ContainsKey(category);
        }

        internal static bool CategoryExists(string machine, string category)
        {
            if (GetPerformanceCounterLib(machine, new CultureInfo(9)).CategoryExists(category))
            {
                return true;
            }
            if (CultureInfo.CurrentCulture.Parent.LCID != 9)
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    if (GetPerformanceCounterLib(machine, info).CategoryExists(category))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal void Close()
        {
            if (this.performanceMonitor != null)
            {
                this.performanceMonitor.Close();
                this.performanceMonitor = null;
            }
            this.CloseTables();
        }

        internal static void CloseAllLibraries()
        {
            if (libraryTable != null)
            {
                foreach (PerformanceCounterLib lib in libraryTable.Values)
                {
                    lib.Close();
                }
                libraryTable = null;
            }
        }

        internal static void CloseAllTables()
        {
            if (libraryTable != null)
            {
                foreach (PerformanceCounterLib lib in libraryTable.Values)
                {
                    lib.CloseTables();
                }
            }
        }

        internal void CloseTables()
        {
            this.nameTable = null;
            this.helpTable = null;
            this.categoryTable = null;
            this.customCategoryTable = null;
        }

        internal static bool CounterExists(string machine, string category, string counter)
        {
            PerformanceCounterLib performanceCounterLib = GetPerformanceCounterLib(machine, new CultureInfo(9));
            bool categoryExists = false;
            bool flag2 = performanceCounterLib.CounterExists(category, counter, ref categoryExists);
            if (!categoryExists && (CultureInfo.CurrentCulture.Parent.LCID != 9))
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    flag2 = GetPerformanceCounterLib(machine, info).CounterExists(category, counter, ref categoryExists);
                    if (flag2)
                    {
                        break;
                    }
                }
            }
            if (!categoryExists)
            {
                throw new InvalidOperationException(SR.GetString("MissingCategory"));
            }
            return flag2;
        }

        private bool CounterExists(string category, string counter, ref bool categoryExists)
        {
            categoryExists = false;
            if (this.CategoryTable.ContainsKey(category))
            {
                categoryExists = true;
                System.Diagnostics.CategoryEntry entry = (System.Diagnostics.CategoryEntry) this.CategoryTable[category];
                for (int i = 0; i < entry.CounterIndexes.Length; i++)
                {
                    int num2 = entry.CounterIndexes[i];
                    string strA = (string) this.NameTable[num2];
                    if (strA == null)
                    {
                        strA = string.Empty;
                    }
                    if (string.Compare(strA, counter, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static void CreateIniFile(string categoryName, string categoryHelp, CounterCreationDataCollection creationData, string[] languageIds)
        {
            new FileIOPermission(PermissionState.Unrestricted).Assert();
            try
            {
                StreamWriter writer = new StreamWriter(IniFilePath, false, Encoding.Unicode);
                try
                {
                    writer.WriteLine("");
                    writer.WriteLine("[info]");
                    writer.Write("drivername");
                    writer.Write("=");
                    writer.WriteLine(categoryName);
                    writer.Write("symbolfile");
                    writer.Write("=");
                    writer.WriteLine(Path.GetFileName(SymbolFilePath));
                    writer.WriteLine("");
                    writer.WriteLine("[languages]");
                    foreach (string str in languageIds)
                    {
                        writer.Write(str);
                        writer.Write("=");
                        writer.Write("language");
                        writer.WriteLine(str);
                    }
                    writer.WriteLine("");
                    writer.WriteLine("[objects]");
                    foreach (string str2 in languageIds)
                    {
                        writer.Write("OBJECT_");
                        writer.Write("1_");
                        writer.Write(str2);
                        writer.Write("_NAME");
                        writer.Write("=");
                        writer.WriteLine(categoryName);
                    }
                    writer.WriteLine("");
                    writer.WriteLine("[text]");
                    foreach (string str3 in languageIds)
                    {
                        writer.Write("OBJECT_");
                        writer.Write("1_");
                        writer.Write(str3);
                        writer.Write("_NAME");
                        writer.Write("=");
                        writer.WriteLine(categoryName);
                        writer.Write("OBJECT_");
                        writer.Write("1_");
                        writer.Write(str3);
                        writer.Write("_HELP");
                        writer.Write("=");
                        if ((categoryHelp == null) || (categoryHelp == string.Empty))
                        {
                            writer.WriteLine(SR.GetString("HelpNotAvailable"));
                        }
                        else
                        {
                            writer.WriteLine(categoryHelp);
                        }
                        int num = 0;
                        foreach (CounterCreationData data in creationData)
                        {
                            num++;
                            writer.WriteLine("");
                            writer.Write("DEVICE_COUNTER_");
                            writer.Write(num.ToString(CultureInfo.InvariantCulture));
                            writer.Write("_");
                            writer.Write(str3);
                            writer.Write("_NAME");
                            writer.Write("=");
                            writer.WriteLine(data.CounterName);
                            writer.Write("DEVICE_COUNTER_");
                            writer.Write(num.ToString(CultureInfo.InvariantCulture));
                            writer.Write("_");
                            writer.Write(str3);
                            writer.Write("_HELP");
                            writer.Write("=");
                            writer.WriteLine(data.CounterHelp);
                        }
                    }
                    writer.WriteLine("");
                }
                finally
                {
                    writer.Close();
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        private static void CreateRegistryEntry(string categoryName, PerformanceCounterCategoryType categoryType, CounterCreationDataCollection creationData, ref bool iniRegistered)
        {
            RegistryKey key = null;
            RegistryKey key2 = null;
            RegistryKey key3 = null;
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true);
                key2 = key.OpenSubKey(categoryName + @"\Performance", true);
                if (key2 == null)
                {
                    key2 = key.CreateSubKey(categoryName + @"\Performance");
                }
                key2.SetValue("Open", "OpenPerformanceData");
                key2.SetValue("Collect", "CollectPerformanceData");
                key2.SetValue("Close", "ClosePerformanceData");
                key2.SetValue("Library", "netfxperf.dll");
                key2.SetValue("IsMultiInstance", (int) categoryType, RegistryValueKind.DWord);
                key2.SetValue("CategoryOptions", 3, RegistryValueKind.DWord);
                string[] strArray = new string[creationData.Count];
                string[] strArray2 = new string[creationData.Count];
                for (int i = 0; i < creationData.Count; i++)
                {
                    strArray[i] = creationData[i].CounterName;
                    strArray2[i] = ((int) creationData[i].CounterType).ToString(CultureInfo.InvariantCulture);
                }
                key3 = key.OpenSubKey(categoryName + @"\Linkage", true);
                if (key3 == null)
                {
                    key3 = key.CreateSubKey(categoryName + @"\Linkage");
                }
                key3.SetValue("Export", new string[] { categoryName });
                key2.SetValue("Counter Types", strArray2);
                key2.SetValue("Counter Names", strArray);
                if (key2.GetValue("First Counter") != null)
                {
                    iniRegistered = true;
                }
                else
                {
                    iniRegistered = false;
                }
            }
            finally
            {
                if (key2 != null)
                {
                    key2.Close();
                }
                if (key3 != null)
                {
                    key3.Close();
                }
                if (key != null)
                {
                    key.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
        }

        private static void CreateSymbolFile(CounterCreationDataCollection creationData)
        {
            new FileIOPermission(PermissionState.Unrestricted).Assert();
            try
            {
                StreamWriter writer = new StreamWriter(SymbolFilePath);
                try
                {
                    writer.Write("#define");
                    writer.Write(" ");
                    writer.Write("OBJECT_");
                    writer.WriteLine("1 0;");
                    for (int i = 1; i <= creationData.Count; i++)
                    {
                        writer.Write("#define");
                        writer.Write(" ");
                        writer.Write("DEVICE_COUNTER_");
                        writer.Write(i.ToString(CultureInfo.InvariantCulture));
                        writer.Write(" ");
                        writer.Write((i * 2).ToString(CultureInfo.InvariantCulture));
                        writer.WriteLine(";");
                    }
                    writer.WriteLine("");
                }
                finally
                {
                    writer.Close();
                }
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        private static void DeleteRegistryEntry(string categoryName)
        {
            RegistryKey key = null;
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true);
                bool flag = false;
                using (RegistryKey key2 = key.OpenSubKey(categoryName, true))
                {
                    if (key2 != null)
                    {
                        if (key2.GetValueNames().Length == 0)
                        {
                            flag = true;
                        }
                        else
                        {
                            key2.DeleteSubKeyTree("Linkage");
                            key2.DeleteSubKeyTree("Performance");
                        }
                    }
                }
                if (flag)
                {
                    key.DeleteSubKeyTree(categoryName);
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
        }

        private static void DeleteTemporaryFiles()
        {
            try
            {
                File.Delete(IniFilePath);
            }
            catch
            {
            }
            try
            {
                File.Delete(SymbolFilePath);
            }
            catch
            {
            }
        }

        internal bool FindCustomCategory(string category, out PerformanceCounterCategoryType categoryType)
        {
            RegistryKey key = null;
            RegistryKey key2 = null;
            categoryType = PerformanceCounterCategoryType.Unknown;
            if (this.customCategoryTable == null)
            {
                Interlocked.CompareExchange<Hashtable>(ref this.customCategoryTable, new Hashtable(StringComparer.OrdinalIgnoreCase), null);
            }
            if (this.customCategoryTable.ContainsKey(category))
            {
                categoryType = (PerformanceCounterCategoryType) this.customCategoryTable[category];
                return true;
            }
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new RegistryPermission(PermissionState.Unrestricted));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Assert();
            try
            {
                string name = @"SYSTEM\CurrentControlSet\Services\" + category + @"\Performance";
                if ((this.machineName == ".") || (string.Compare(this.machineName, ComputerName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    key = Registry.LocalMachine.OpenSubKey(name);
                }
                else
                {
                    key2 = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, @"\\" + this.machineName);
                    if (key2 != null)
                    {
                        try
                        {
                            key = key2.OpenSubKey(name);
                        }
                        catch (SecurityException)
                        {
                            categoryType = PerformanceCounterCategoryType.Unknown;
                            this.customCategoryTable[category] = (PerformanceCounterCategoryType) categoryType;
                            return false;
                        }
                    }
                }
                if (key != null)
                {
                    object obj2 = key.GetValue("Library");
                    if (((obj2 != null) && (obj2 is string)) && (string.Compare((string) obj2, "netfxperf.dll", StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        object obj3 = key.GetValue("IsMultiInstance");
                        if (obj3 != null)
                        {
                            categoryType = (PerformanceCounterCategoryType) obj3;
                            if ((categoryType < PerformanceCounterCategoryType.Unknown) || (categoryType > PerformanceCounterCategoryType.MultiInstance))
                            {
                                categoryType = PerformanceCounterCategoryType.Unknown;
                            }
                        }
                        else
                        {
                            categoryType = PerformanceCounterCategoryType.Unknown;
                        }
                        object obj4 = key.GetValue("First Counter");
                        if (obj4 != null)
                        {
                            int num1 = (int) obj4;
                            this.customCategoryTable[category] = (PerformanceCounterCategoryType) categoryType;
                            return true;
                        }
                    }
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
                if (key2 != null)
                {
                    key2.Close();
                }
                PermissionSet.RevertAssert();
            }
            return false;
        }

        internal string[] GetCategories()
        {
            ICollection keys = this.CategoryTable.Keys;
            string[] array = new string[keys.Count];
            keys.CopyTo(array, 0);
            return array;
        }

        internal static string[] GetCategories(string machineName)
        {
            for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
            {
                string[] categories = GetPerformanceCounterLib(machineName, info).GetCategories();
                if (categories.Length != 0)
                {
                    return categories;
                }
            }
            return GetPerformanceCounterLib(machineName, new CultureInfo(9)).GetCategories();
        }

        private string GetCategoryHelp(string category)
        {
            System.Diagnostics.CategoryEntry entry = (System.Diagnostics.CategoryEntry) this.CategoryTable[category];
            if (entry == null)
            {
                return null;
            }
            return (string) this.HelpTable[entry.HelpIndex];
        }

        internal static string GetCategoryHelp(string machine, string category)
        {
            string categoryHelp;
            if (CultureInfo.CurrentCulture.Parent.LCID != 9)
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    categoryHelp = GetPerformanceCounterLib(machine, info).GetCategoryHelp(category);
                    if (categoryHelp != null)
                    {
                        return categoryHelp;
                    }
                }
            }
            categoryHelp = GetPerformanceCounterLib(machine, new CultureInfo(9)).GetCategoryHelp(category);
            if (categoryHelp == null)
            {
                throw new InvalidOperationException(SR.GetString("MissingCategory"));
            }
            return categoryHelp;
        }

        private CategorySample GetCategorySample(string category)
        {
            System.Diagnostics.CategoryEntry entry = (System.Diagnostics.CategoryEntry) this.CategoryTable[category];
            if (entry == null)
            {
                return null;
            }
            byte[] performanceData = this.GetPerformanceData(entry.NameIndex.ToString(CultureInfo.InvariantCulture));
            if (performanceData == null)
            {
                throw new InvalidOperationException(SR.GetString("CantReadCategory", new object[] { category }));
            }
            return new CategorySample(performanceData, entry, this);
        }

        internal static CategorySample GetCategorySample(string machine, string category)
        {
            CategorySample categorySample = GetPerformanceCounterLib(machine, new CultureInfo(9)).GetCategorySample(category);
            if ((categorySample == null) && (CultureInfo.CurrentCulture.Parent.LCID != 9))
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    categorySample = GetPerformanceCounterLib(machine, info).GetCategorySample(category);
                    if (categorySample != null)
                    {
                        return categorySample;
                    }
                }
            }
            if (categorySample == null)
            {
                throw new InvalidOperationException(SR.GetString("MissingCategory"));
            }
            return categorySample;
        }

        internal static PerformanceCounterCategoryType GetCategoryType(string machine, string category)
        {
            PerformanceCounterCategoryType unknown = PerformanceCounterCategoryType.Unknown;
            if (!GetPerformanceCounterLib(machine, new CultureInfo(9)).FindCustomCategory(category, out unknown) && (CultureInfo.CurrentCulture.Parent.LCID != 9))
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    if (GetPerformanceCounterLib(machine, info).FindCustomCategory(category, out unknown))
                    {
                        return unknown;
                    }
                }
            }
            return unknown;
        }

        internal static string GetCounterHelp(string machine, string category, string counter)
        {
            string str;
            bool categoryExists = false;
            if (CultureInfo.CurrentCulture.Parent.LCID != 9)
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    str = GetPerformanceCounterLib(machine, info).GetCounterHelp(category, counter, ref categoryExists);
                    if (categoryExists)
                    {
                        return str;
                    }
                }
            }
            str = GetPerformanceCounterLib(machine, new CultureInfo(9)).GetCounterHelp(category, counter, ref categoryExists);
            if (!categoryExists)
            {
                throw new InvalidOperationException(SR.GetString("MissingCategoryDetail", new object[] { category }));
            }
            return str;
        }

        private string GetCounterHelp(string category, string counter, ref bool categoryExists)
        {
            categoryExists = false;
            System.Diagnostics.CategoryEntry entry = (System.Diagnostics.CategoryEntry) this.CategoryTable[category];
            if (entry == null)
            {
                return null;
            }
            categoryExists = true;
            int num = -1;
            for (int i = 0; i < entry.CounterIndexes.Length; i++)
            {
                int num3 = entry.CounterIndexes[i];
                string strA = (string) this.NameTable[num3];
                if (strA == null)
                {
                    strA = string.Empty;
                }
                if (string.Compare(strA, counter, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    num = entry.HelpIndexes[i];
                    break;
                }
            }
            if (num == -1)
            {
                throw new InvalidOperationException(SR.GetString("MissingCounter", new object[] { counter }));
            }
            string str2 = (string) this.HelpTable[num];
            if (str2 == null)
            {
                return string.Empty;
            }
            return str2;
        }

        internal string GetCounterName(int index)
        {
            if (this.NameTable.ContainsKey(index))
            {
                return (string) this.NameTable[index];
            }
            return "";
        }

        internal static string[] GetCounters(string machine, string category)
        {
            PerformanceCounterLib performanceCounterLib = GetPerformanceCounterLib(machine, new CultureInfo(9));
            bool categoryExists = false;
            string[] counters = performanceCounterLib.GetCounters(category, ref categoryExists);
            if (!categoryExists && (CultureInfo.CurrentCulture.Parent.LCID != 9))
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    counters = GetPerformanceCounterLib(machine, info).GetCounters(category, ref categoryExists);
                    if (categoryExists)
                    {
                        return counters;
                    }
                }
            }
            if (!categoryExists)
            {
                throw new InvalidOperationException(SR.GetString("MissingCategory"));
            }
            return counters;
        }

        private string[] GetCounters(string category, ref bool categoryExists)
        {
            categoryExists = false;
            System.Diagnostics.CategoryEntry entry = (System.Diagnostics.CategoryEntry) this.CategoryTable[category];
            if (entry == null)
            {
                return null;
            }
            categoryExists = true;
            int index = 0;
            string[] sourceArray = new string[entry.CounterIndexes.Length];
            for (int i = 0; i < sourceArray.Length; i++)
            {
                int num3 = entry.CounterIndexes[i];
                string str = (string) this.NameTable[num3];
                if ((str != null) && (str != string.Empty))
                {
                    sourceArray[index] = str;
                    index++;
                }
            }
            if (index < sourceArray.Length)
            {
                string[] destinationArray = new string[index];
                Array.Copy(sourceArray, destinationArray, index);
                sourceArray = destinationArray;
            }
            return sourceArray;
        }

        private static string[] GetLanguageIds()
        {
            RegistryKey key = null;
            string[] subKeyNames = new string[0];
            new RegistryPermission(PermissionState.Unrestricted).Assert();
            try
            {
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Perflib");
                if (key != null)
                {
                    subKeyNames = key.GetSubKeyNames();
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
                CodeAccessPermission.RevertAssert();
            }
            return subKeyNames;
        }

        internal static PerformanceCounterLib GetPerformanceCounterLib(string machineName, CultureInfo culture)
        {
            SharedUtils.CheckEnvironment();
            string lcid = culture.LCID.ToString("X3", CultureInfo.InvariantCulture);
            if (machineName.CompareTo(".") == 0)
            {
                machineName = ComputerName.ToLower(CultureInfo.InvariantCulture);
            }
            else
            {
                machineName = machineName.ToLower(CultureInfo.InvariantCulture);
            }
            if (libraryTable == null)
            {
                lock (InternalSyncObject)
                {
                    if (libraryTable == null)
                    {
                        libraryTable = new Hashtable();
                    }
                }
            }
            string key = machineName + ":" + lcid;
            if (libraryTable.Contains(key))
            {
                return (PerformanceCounterLib) libraryTable[key];
            }
            PerformanceCounterLib lib = new PerformanceCounterLib(machineName, lcid);
            libraryTable[key] = lib;
            return lib;
        }

        internal byte[] GetPerformanceData(string item)
        {
            if (this.performanceMonitor == null)
            {
                lock (InternalSyncObject)
                {
                    if (this.performanceMonitor == null)
                    {
                        this.performanceMonitor = new PerformanceMonitor(this.machineName);
                    }
                }
            }
            return this.performanceMonitor.GetData(item);
        }

        private Hashtable GetStringTable(bool isHelp)
        {
            Hashtable hashtable;
            RegistryKey performanceData;
            PermissionSet set = new PermissionSet(PermissionState.None);
            set.AddPermission(new RegistryPermission(PermissionState.Unrestricted));
            set.AddPermission(new SecurityPermission(SecurityPermissionFlag.UnmanagedCode));
            set.Assert();
            if (string.Compare(this.machineName, ComputerName, StringComparison.OrdinalIgnoreCase) == 0)
            {
                performanceData = Registry.PerformanceData;
            }
            else
            {
                performanceData = RegistryKey.OpenRemoteBaseKey(RegistryHive.PerformanceData, this.machineName);
            }
            try
            {
                string[] strArray = null;
                int num = 14;
                int millisecondsTimeout = 0;
                while (num > 0)
                {
                    try
                    {
                        if (!isHelp)
                        {
                            strArray = (string[]) performanceData.GetValue("Counter " + this.perfLcid);
                        }
                        else
                        {
                            strArray = (string[]) performanceData.GetValue("Explain " + this.perfLcid);
                        }
                        if ((strArray != null) && (strArray.Length != 0))
                        {
                            break;
                        }
                        num--;
                        if (millisecondsTimeout == 0)
                        {
                            millisecondsTimeout = 10;
                        }
                        else
                        {
                            Thread.Sleep(millisecondsTimeout);
                            millisecondsTimeout *= 2;
                        }
                        continue;
                    }
                    catch (IOException)
                    {
                        strArray = null;
                        break;
                    }
                    catch (InvalidCastException)
                    {
                        strArray = null;
                        break;
                    }
                }
                if (strArray == null)
                {
                    return new Hashtable();
                }
                hashtable = new Hashtable(strArray.Length / 2);
                for (int i = 0; i < (strArray.Length / 2); i++)
                {
                    int num4;
                    string str = strArray[(i * 2) + 1];
                    if (str == null)
                    {
                        str = string.Empty;
                    }
                    if (!int.TryParse(strArray[i * 2], NumberStyles.Integer, CultureInfo.InvariantCulture, out num4))
                    {
                        if (isHelp)
                        {
                            throw new InvalidOperationException(SR.GetString("CategoryHelpCorrupt", new object[] { strArray[i * 2] }));
                        }
                        throw new InvalidOperationException(SR.GetString("CounterNameCorrupt", new object[] { strArray[i * 2] }));
                    }
                    hashtable[num4] = str;
                }
            }
            finally
            {
                performanceData.Close();
            }
            return hashtable;
        }

        internal static bool IsBaseCounter(int type)
        {
            if (((type != 0x40030402) && (type != 0x42030500)) && ((type != 0x40030403) && (type != 0x40030500)))
            {
                return (type == 0x40030401);
            }
            return true;
        }

        private bool IsCustomCategory(string category)
        {
            PerformanceCounterCategoryType type;
            return this.FindCustomCategory(category, out type);
        }

        internal static bool IsCustomCategory(string machine, string category)
        {
            if (GetPerformanceCounterLib(machine, new CultureInfo(9)).IsCustomCategory(category))
            {
                return true;
            }
            if (CultureInfo.CurrentCulture.Parent.LCID != 9)
            {
                for (CultureInfo info = CultureInfo.CurrentCulture; info != CultureInfo.InvariantCulture; info = info.Parent)
                {
                    if (GetPerformanceCounterLib(machine, info).IsCustomCategory(category))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void RegisterCategory(string categoryName, PerformanceCounterCategoryType categoryType, string categoryHelp, CounterCreationDataCollection creationData)
        {
            try
            {
                bool iniRegistered = false;
                CreateRegistryEntry(categoryName, categoryType, creationData, ref iniRegistered);
                if (!iniRegistered)
                {
                    string[] languageIds = GetLanguageIds();
                    CreateIniFile(categoryName, categoryHelp, creationData, languageIds);
                    CreateSymbolFile(creationData);
                    RegisterFiles(IniFilePath, false);
                }
                CloseAllTables();
                CloseAllLibraries();
            }
            finally
            {
                DeleteTemporaryFiles();
            }
        }

        private static void RegisterFiles(string arg0, bool unregister)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo {
                UseShellExecute = false,
                CreateNoWindow = true,
                ErrorDialog = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = Environment.SystemDirectory
            };
            if (unregister)
            {
                startInfo.FileName = Environment.SystemDirectory + @"\unlodctr.exe";
            }
            else
            {
                startInfo.FileName = Environment.SystemDirectory + @"\lodctr.exe";
            }
            int error = 0;
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert();
            try
            {
                startInfo.Arguments = "\"" + arg0 + "\"";
                Process process = Process.Start(startInfo);
                process.WaitForExit();
                error = process.ExitCode;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            if (error == 5)
            {
                throw new UnauthorizedAccessException(SR.GetString("CantChangeCategoryRegistration", new object[] { arg0 }));
            }
            if (unregister && (error == 2))
            {
                error = 0;
            }
            if (error != 0)
            {
                throw SharedUtils.CreateSafeWin32Exception(error);
            }
        }

        internal static void UnregisterCategory(string categoryName)
        {
            RegisterFiles(categoryName, true);
            DeleteRegistryEntry(categoryName);
            CloseAllTables();
            CloseAllLibraries();
        }

        private Hashtable CategoryTable
        {
            get
            {
                if (this.categoryTable == null)
                {
                    lock (this.CategoryTableLock)
                    {
                        if (this.categoryTable == null)
                        {
                            fixed (byte* numRef = this.GetPerformanceData("Global"))
                            {
                                IntPtr ptr = new IntPtr((void*) numRef);
                                Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK structure = new Microsoft.Win32.NativeMethods.PERF_DATA_BLOCK();
                                Marshal.PtrToStructure(ptr, structure);
                                ptr = (IntPtr) (((long) ptr) + structure.HeaderLength);
                                int numObjectTypes = structure.NumObjectTypes;
                                long num2 = ((long) new IntPtr((void*) numRef)) + structure.TotalByteLength;
                                Hashtable hashtable = new Hashtable(numObjectTypes, StringComparer.OrdinalIgnoreCase);
                                for (int i = 0; (i < numObjectTypes) && (((long) ptr) < num2); i++)
                                {
                                    Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE perf_object_type = new Microsoft.Win32.NativeMethods.PERF_OBJECT_TYPE();
                                    Marshal.PtrToStructure(ptr, perf_object_type);
                                    System.Diagnostics.CategoryEntry entry = new System.Diagnostics.CategoryEntry(perf_object_type);
                                    IntPtr ptr2 = (IntPtr) (((long) ptr) + perf_object_type.TotalByteLength);
                                    ptr = (IntPtr) (((long) ptr) + perf_object_type.HeaderLength);
                                    int index = 0;
                                    int counterNameTitleIndex = -1;
                                    for (int j = 0; j < entry.CounterIndexes.Length; j++)
                                    {
                                        Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION perf_counter_definition = new Microsoft.Win32.NativeMethods.PERF_COUNTER_DEFINITION();
                                        Marshal.PtrToStructure(ptr, perf_counter_definition);
                                        if (perf_counter_definition.CounterNameTitleIndex != counterNameTitleIndex)
                                        {
                                            entry.CounterIndexes[index] = perf_counter_definition.CounterNameTitleIndex;
                                            entry.HelpIndexes[index] = perf_counter_definition.CounterHelpTitleIndex;
                                            counterNameTitleIndex = perf_counter_definition.CounterNameTitleIndex;
                                            index++;
                                        }
                                        ptr = (IntPtr) (((long) ptr) + perf_counter_definition.ByteLength);
                                    }
                                    if (index < entry.CounterIndexes.Length)
                                    {
                                        int[] destinationArray = new int[index];
                                        int[] numArray2 = new int[index];
                                        Array.Copy(entry.CounterIndexes, destinationArray, index);
                                        Array.Copy(entry.HelpIndexes, numArray2, index);
                                        entry.CounterIndexes = destinationArray;
                                        entry.HelpIndexes = numArray2;
                                    }
                                    string str = (string) this.NameTable[entry.NameIndex];
                                    if (str != null)
                                    {
                                        hashtable[str] = entry;
                                    }
                                    ptr = ptr2;
                                }
                                this.categoryTable = hashtable;
                            }
                        }
                    }
                }
                return this.categoryTable;
            }
        }

        internal static string ComputerName
        {
            get
            {
                if (computerName == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (computerName == null)
                        {
                            StringBuilder lpBuffer = new StringBuilder(0x100);
                            Microsoft.Win32.SafeNativeMethods.GetComputerName(lpBuffer, new int[] { lpBuffer.Capacity });
                            computerName = lpBuffer.ToString();
                        }
                    }
                }
                return computerName;
            }
        }

        internal Hashtable HelpTable
        {
            get
            {
                if (this.helpTable == null)
                {
                    lock (this.HelpTableLock)
                    {
                        if (this.helpTable == null)
                        {
                            this.helpTable = this.GetStringTable(true);
                        }
                    }
                }
                return this.helpTable;
            }
        }

        private static string IniFilePath
        {
            get
            {
                if (iniFilePath == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (iniFilePath == null)
                        {
                            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                            try
                            {
                                iniFilePath = Path.GetTempFileName();
                            }
                            finally
                            {
                                CodeAccessPermission.RevertAssert();
                            }
                        }
                    }
                }
                return iniFilePath;
            }
        }

        private static object InternalSyncObject
        {
            get
            {
                if (s_InternalSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref s_InternalSyncObject, obj2, null);
                }
                return s_InternalSyncObject;
            }
        }

        internal Hashtable NameTable
        {
            get
            {
                if (this.nameTable == null)
                {
                    lock (this.NameTableLock)
                    {
                        if (this.nameTable == null)
                        {
                            this.nameTable = this.GetStringTable(false);
                        }
                    }
                }
                return this.nameTable;
            }
        }

        private static string SymbolFilePath
        {
            get
            {
                if (symbolFilePath == null)
                {
                    lock (InternalSyncObject)
                    {
                        if (symbolFilePath == null)
                        {
                            new EnvironmentPermission(PermissionState.Unrestricted).Assert();
                            string tempPath = Path.GetTempPath();
                            CodeAccessPermission.RevertAssert();
                            PermissionSet set = new PermissionSet(PermissionState.None);
                            set.AddPermission(new EnvironmentPermission(PermissionState.Unrestricted));
                            set.AddPermission(new FileIOPermission(FileIOPermissionAccess.Write, tempPath));
                            set.Assert();
                            try
                            {
                                symbolFilePath = Path.GetTempFileName();
                            }
                            finally
                            {
                                PermissionSet.RevertAssert();
                            }
                        }
                    }
                }
                return symbolFilePath;
            }
        }
    }
}

