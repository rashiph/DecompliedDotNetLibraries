namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    public class PerformanceCounterInstaller : ComponentInstaller
    {
        private string categoryHelp = string.Empty;
        private string categoryName = string.Empty;
        private PerformanceCounterCategoryType categoryType = PerformanceCounterCategoryType.Unknown;
        private CounterCreationDataCollection counters = new CounterCreationDataCollection();
        private const string PerfShimName = "netfxperf.dll";
        private const string ServicePath = @"SYSTEM\CurrentControlSet\Services";
        private System.Configuration.Install.UninstallAction uninstallAction;

        internal static void CheckValidCategory(string categoryName)
        {
            if (categoryName == null)
            {
                throw new ArgumentNullException("categoryName");
            }
            if (!CheckValidId(categoryName))
            {
                throw new ArgumentException(Res.GetString("PerfInvalidCategoryName", new object[] { 1, 0xfd }));
            }
        }

        internal static bool CheckValidId(string id)
        {
            if ((id.Length == 0) || (id.Length > 0xfd))
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

        public override void CopyFromComponent(IComponent component)
        {
            if (!(component is PerformanceCounter))
            {
                throw new ArgumentException(Res.GetString("NotAPerformanceCounter"));
            }
            PerformanceCounter counter = (PerformanceCounter) component;
            if ((counter.CategoryName == null) || (counter.CategoryName.Length == 0))
            {
                throw new ArgumentException(Res.GetString("IncompletePerformanceCounter"));
            }
            if (!this.CategoryName.Equals(counter.CategoryName) && !string.IsNullOrEmpty(this.CategoryName))
            {
                throw new ArgumentException(Res.GetString("NewCategory"));
            }
            PerformanceCounterType counterType = PerformanceCounterType.NumberOfItems32;
            string counterHelp = string.Empty;
            if (string.IsNullOrEmpty(this.CategoryName))
            {
                this.CategoryName = counter.CategoryName;
            }
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                string machineName = counter.MachineName;
                if (PerformanceCounterCategory.Exists(this.CategoryName, machineName))
                {
                    string name = @"SYSTEM\CurrentControlSet\Services\" + this.CategoryName + @"\Performance";
                    RegistryKey key = null;
                    try
                    {
                        if ((machineName == ".") || (string.Compare(machineName, SystemInformation.ComputerName, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            key = Registry.LocalMachine.OpenSubKey(name);
                        }
                        else
                        {
                            key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, @"\\" + machineName).OpenSubKey(name);
                        }
                        if (key == null)
                        {
                            throw new ArgumentException(Res.GetString("NotCustomPerformanceCategory"));
                        }
                        object obj2 = key.GetValue("Library");
                        if (((obj2 == null) || !(obj2 is string)) || (string.Compare((string) obj2, "netfxperf.dll", StringComparison.OrdinalIgnoreCase) != 0))
                        {
                            throw new ArgumentException(Res.GetString("NotCustomPerformanceCategory"));
                        }
                        PerformanceCounterCategory category = new PerformanceCounterCategory(this.CategoryName, machineName);
                        this.CategoryHelp = category.CategoryHelp;
                        if (category.CounterExists(counter.CounterName))
                        {
                            counterType = counter.CounterType;
                            counterHelp = counter.CounterHelp;
                        }
                        this.CategoryType = category.CategoryType;
                    }
                    finally
                    {
                        if (key != null)
                        {
                            key.Close();
                        }
                    }
                }
            }
            CounterCreationData data = new CounterCreationData(counter.CounterName, counterHelp, counterType);
            this.Counters.Add(data);
        }

        private void DoRollback(IDictionary state)
        {
            base.Context.LogMessage(Res.GetString("RestoringPerformanceCounter", new object[] { this.CategoryName }));
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true))
            {
                RegistryKey key2 = null;
                if ((bool) state["categoryKeyExisted"])
                {
                    key2 = key.OpenSubKey(this.CategoryName, true);
                    if (key2 == null)
                    {
                        key2 = key.CreateSubKey(this.CategoryName);
                    }
                    key2.DeleteSubKeyTree("Performance");
                    SerializableRegistryKey key3 = (SerializableRegistryKey) state["performanceKeyData"];
                    if (key3 != null)
                    {
                        RegistryKey baseKey = key2.CreateSubKey("Performance");
                        key3.CopyToRegistry(baseKey);
                        baseKey.Close();
                    }
                    key2.DeleteSubKeyTree("Linkage");
                    SerializableRegistryKey key5 = (SerializableRegistryKey) state["linkageKeyData"];
                    if (key5 != null)
                    {
                        RegistryKey key6 = key2.CreateSubKey("Linkage");
                        key5.CopyToRegistry(key6);
                        key6.Close();
                    }
                }
                else
                {
                    key2 = key.OpenSubKey(this.CategoryName);
                    if (key2 != null)
                    {
                        key2.Close();
                        key2 = null;
                        key.DeleteSubKeyTree(this.CategoryName);
                    }
                }
                if (key2 != null)
                {
                    key2.Close();
                }
            }
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            base.Context.LogMessage(Res.GetString("CreatingPerformanceCounter", new object[] { this.CategoryName }));
            RegistryKey key = null;
            RegistryKey keyToSave = null;
            RegistryKey key3 = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services", true);
            stateSaver["categoryKeyExisted"] = false;
            try
            {
                if (key3 != null)
                {
                    key = key3.OpenSubKey(this.CategoryName, true);
                    if (key != null)
                    {
                        stateSaver["categoryKeyExisted"] = true;
                        keyToSave = key.OpenSubKey("Performance");
                        if (keyToSave != null)
                        {
                            stateSaver["performanceKeyData"] = new SerializableRegistryKey(keyToSave);
                            keyToSave.Close();
                            key.DeleteSubKeyTree("Performance");
                        }
                        keyToSave = key.OpenSubKey("Linkage");
                        if (keyToSave != null)
                        {
                            stateSaver["linkageKeyData"] = new SerializableRegistryKey(keyToSave);
                            keyToSave.Close();
                            key.DeleteSubKeyTree("Linkage");
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
                if (key3 != null)
                {
                    key3.Close();
                }
            }
            if (PerformanceCounterCategory.Exists(this.CategoryName))
            {
                PerformanceCounterCategory.Delete(this.CategoryName);
            }
            PerformanceCounterCategory.Create(this.CategoryName, this.CategoryHelp, this.categoryType, this.Counters);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            this.DoRollback(savedState);
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            if (this.UninstallAction == System.Configuration.Install.UninstallAction.Remove)
            {
                base.Context.LogMessage(Res.GetString("RemovingPerformanceCounter", new object[] { this.CategoryName }));
                PerformanceCounterCategory.Delete(this.CategoryName);
            }
        }

        [DefaultValue(""), ResDescription("PCI_CategoryHelp")]
        public string CategoryHelp
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.categoryHelp;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                this.categoryHelp = value;
            }
        }

        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultValue(""), ResDescription("PCCategoryName")]
        public string CategoryName
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
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
                CheckValidCategory(value);
                this.categoryName = value;
            }
        }

        [ComVisible(false), DefaultValue(-1), ResDescription("PCI_IsMultiInstance")]
        public PerformanceCounterCategoryType CategoryType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.categoryType;
            }
            set
            {
                if ((value < PerformanceCounterCategoryType.Unknown) || (value > PerformanceCounterCategoryType.MultiInstance))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(PerformanceCounterCategoryType));
                }
                this.categoryType = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), ResDescription("PCI_Counters")]
        public CounterCreationDataCollection Counters
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.counters;
            }
        }

        [ResDescription("PCI_UninstallAction"), DefaultValue(0)]
        public System.Configuration.Install.UninstallAction UninstallAction
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.uninstallAction;
            }
            set
            {
                if (!Enum.IsDefined(typeof(System.Configuration.Install.UninstallAction), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.Configuration.Install.UninstallAction));
                }
                this.uninstallAction = value;
            }
        }
    }
}

