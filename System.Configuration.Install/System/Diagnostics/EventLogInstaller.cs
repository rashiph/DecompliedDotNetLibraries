namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.Runtime;
    using System.Runtime.InteropServices;

    public class EventLogInstaller : ComponentInstaller
    {
        private EventSourceCreationData sourceData = new EventSourceCreationData(null, null);
        private System.Configuration.Install.UninstallAction uninstallAction;

        public override void CopyFromComponent(IComponent component)
        {
            EventLog log = component as EventLog;
            if (log == null)
            {
                throw new ArgumentException(Res.GetString("NotAnEventLog"));
            }
            if (((log.Log == null) || (log.Log == string.Empty)) || ((log.Source == null) || (log.Source == string.Empty)))
            {
                throw new ArgumentException(Res.GetString("IncompleteEventLog"));
            }
            this.Log = log.Log;
            this.Source = log.Source;
        }

        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);
            base.Context.LogMessage(Res.GetString("CreatingEventLog", new object[] { this.Source, this.Log }));
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                throw new PlatformNotSupportedException(Res.GetString("WinNTRequired"));
            }
            stateSaver["baseInstalledAndPlatformOK"] = true;
            bool flag = EventLog.Exists(this.Log, ".");
            stateSaver["logExists"] = flag;
            bool flag2 = EventLog.SourceExists(this.Source, ".");
            stateSaver["alreadyRegistered"] = flag2;
            if (!flag2 || (EventLog.LogNameFromSourceName(this.Source, ".") != this.Log))
            {
                EventLog.CreateEventSource(this.sourceData);
            }
        }

        public override bool IsEquivalentInstaller(ComponentInstaller otherInstaller)
        {
            EventLogInstaller installer = otherInstaller as EventLogInstaller;
            if (installer == null)
            {
                return false;
            }
            return (installer.Source == this.Source);
        }

        public override void Rollback(IDictionary savedState)
        {
            base.Rollback(savedState);
            base.Context.LogMessage(Res.GetString("RestoringEventLog", new object[] { this.Source }));
            if (savedState["baseInstalledAndPlatformOK"] != null)
            {
                if (!((bool) savedState["logExists"]))
                {
                    EventLog.Delete(this.Log, ".");
                }
                else
                {
                    bool flag2;
                    object obj2 = savedState["alreadyRegistered"];
                    if (obj2 == null)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        flag2 = (bool) obj2;
                    }
                    if (!flag2 && EventLog.SourceExists(this.Source, "."))
                    {
                        EventLog.DeleteEventSource(this.Source, ".");
                    }
                }
            }
        }

        public override void Uninstall(IDictionary savedState)
        {
            base.Uninstall(savedState);
            if (this.UninstallAction == System.Configuration.Install.UninstallAction.Remove)
            {
                base.Context.LogMessage(Res.GetString("RemovingEventLog", new object[] { this.Source }));
                if (EventLog.SourceExists(this.Source, "."))
                {
                    if (string.Compare(this.Log, this.Source, StringComparison.OrdinalIgnoreCase) != 0)
                    {
                        EventLog.DeleteEventSource(this.Source, ".");
                    }
                }
                else
                {
                    base.Context.LogMessage(Res.GetString("LocalSourceNotRegisteredWarning", new object[] { this.Source }));
                }
                RegistryKey localMachine = Registry.LocalMachine;
                RegistryKey key2 = null;
                try
                {
                    localMachine = localMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\EventLog", false);
                    if (localMachine != null)
                    {
                        key2 = localMachine.OpenSubKey(this.Log, false);
                    }
                    if (key2 != null)
                    {
                        string[] subKeyNames = key2.GetSubKeyNames();
                        if (((subKeyNames == null) || (subKeyNames.Length == 0)) || ((subKeyNames.Length == 1) && (string.Compare(subKeyNames[0], this.Log, StringComparison.OrdinalIgnoreCase) == 0)))
                        {
                            base.Context.LogMessage(Res.GetString("DeletingEventLog", new object[] { this.Log }));
                            EventLog.Delete(this.Log, ".");
                        }
                    }
                }
                finally
                {
                    if (localMachine != null)
                    {
                        localMachine.Close();
                    }
                    if (key2 != null)
                    {
                        key2.Close();
                    }
                }
            }
        }

        [ResDescription("Desc_CategoryCount"), ComVisible(false)]
        public int CategoryCount
        {
            get
            {
                return this.sourceData.CategoryCount;
            }
            set
            {
                this.sourceData.CategoryCount = value;
            }
        }

        [ResDescription("Desc_CategoryResourceFile"), Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ComVisible(false), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string CategoryResourceFile
        {
            get
            {
                return this.sourceData.CategoryResourceFile;
            }
            set
            {
                this.sourceData.CategoryResourceFile = value;
            }
        }

        [ResDescription("Desc_Log"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string Log
        {
            get
            {
                if ((this.sourceData.LogName == null) && (this.sourceData.Source != null))
                {
                    this.sourceData.LogName = EventLog.LogNameFromSourceName(this.sourceData.Source, ".");
                }
                return this.sourceData.LogName;
            }
            set
            {
                this.sourceData.LogName = value;
            }
        }

        [ComVisible(false), Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResDescription("Desc_MessageResourceFile")]
        public string MessageResourceFile
        {
            get
            {
                return this.sourceData.MessageResourceFile;
            }
            set
            {
                this.sourceData.MessageResourceFile = value;
            }
        }

        [Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResDescription("Desc_ParameterResourceFile"), ComVisible(false)]
        public string ParameterResourceFile
        {
            get
            {
                return this.sourceData.ParameterResourceFile;
            }
            set
            {
                this.sourceData.ParameterResourceFile = value;
            }
        }

        [TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), ResDescription("Desc_Source")]
        public string Source
        {
            get
            {
                return this.sourceData.Source;
            }
            set
            {
                this.sourceData.Source = value;
            }
        }

        [DefaultValue(0), ResDescription("Desc_UninstallAction")]
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

