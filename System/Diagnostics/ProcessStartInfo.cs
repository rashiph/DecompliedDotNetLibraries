namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [TypeConverter(typeof(ExpandableObjectConverter)), HostProtection(SecurityAction.LinkDemand, SharedState=true, SelfAffectingProcessMgmt=true), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class ProcessStartInfo
    {
        private string arguments;
        private bool createNoWindow;
        private string directory;
        private string domain;
        internal StringDictionary environmentVariables;
        private bool errorDialog;
        private IntPtr errorDialogParentHandle;
        private string fileName;
        private bool loadUserProfile;
        private SecureString password;
        private bool redirectStandardError;
        private bool redirectStandardInput;
        private bool redirectStandardOutput;
        private Encoding standardErrorEncoding;
        private Encoding standardOutputEncoding;
        private string userName;
        private bool useShellExecute;
        private string verb;
        private WeakReference weakParentProcess;
        private ProcessWindowStyle windowStyle;

        public ProcessStartInfo()
        {
            this.useShellExecute = true;
        }

        internal ProcessStartInfo(Process parent)
        {
            this.useShellExecute = true;
            this.weakParentProcess = new WeakReference(parent);
        }

        public ProcessStartInfo(string fileName)
        {
            this.useShellExecute = true;
            this.fileName = fileName;
        }

        public ProcessStartInfo(string fileName, string arguments)
        {
            this.useShellExecute = true;
            this.fileName = fileName;
            this.arguments = arguments;
        }

        [SettingsBindable(true), DefaultValue(""), MonitoringDescription("ProcessArguments"), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), NotifyParentProperty(true)]
        public string Arguments
        {
            get
            {
                if (this.arguments == null)
                {
                    return string.Empty;
                }
                return this.arguments;
            }
            set
            {
                this.arguments = value;
            }
        }

        [DefaultValue(false), MonitoringDescription("ProcessCreateNoWindow"), NotifyParentProperty(true)]
        public bool CreateNoWindow
        {
            get
            {
                return this.createNoWindow;
            }
            set
            {
                this.createNoWindow = value;
            }
        }

        [NotifyParentProperty(true)]
        public string Domain
        {
            get
            {
                if (this.domain == null)
                {
                    return string.Empty;
                }
                return this.domain;
            }
            set
            {
                this.domain = value;
            }
        }

        [Editor("System.Diagnostics.Design.StringDictionaryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content), DefaultValue((string) null), MonitoringDescription("ProcessEnvironmentVariables"), NotifyParentProperty(true)]
        public StringDictionary EnvironmentVariables
        {
            get
            {
                if (this.environmentVariables == null)
                {
                    this.environmentVariables = new StringDictionaryWithComparer();
                    if (((this.weakParentProcess == null) || !this.weakParentProcess.IsAlive) || ((((Component) this.weakParentProcess.Target).Site == null) || !((Component) this.weakParentProcess.Target).Site.DesignMode))
                    {
                        foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
                        {
                            this.environmentVariables.Add((string) entry.Key, (string) entry.Value);
                        }
                    }
                }
                return this.environmentVariables;
            }
        }

        [DefaultValue(false), NotifyParentProperty(true), MonitoringDescription("ProcessErrorDialog")]
        public bool ErrorDialog
        {
            get
            {
                return this.errorDialog;
            }
            set
            {
                this.errorDialog = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr ErrorDialogParentHandle
        {
            get
            {
                return this.errorDialogParentHandle;
            }
            set
            {
                this.errorDialogParentHandle = value;
            }
        }

        [NotifyParentProperty(true), DefaultValue(""), Editor("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MonitoringDescription("ProcessFileName"), SettingsBindable(true), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName
        {
            get
            {
                if (this.fileName == null)
                {
                    return string.Empty;
                }
                return this.fileName;
            }
            set
            {
                this.fileName = value;
            }
        }

        [NotifyParentProperty(true)]
        public bool LoadUserProfile
        {
            get
            {
                return this.loadUserProfile;
            }
            set
            {
                this.loadUserProfile = value;
            }
        }

        public SecureString Password
        {
            get
            {
                return this.password;
            }
            set
            {
                this.password = value;
            }
        }

        [MonitoringDescription("ProcessRedirectStandardError"), DefaultValue(false), NotifyParentProperty(true)]
        public bool RedirectStandardError
        {
            get
            {
                return this.redirectStandardError;
            }
            set
            {
                this.redirectStandardError = value;
            }
        }

        [DefaultValue(false), MonitoringDescription("ProcessRedirectStandardInput"), NotifyParentProperty(true)]
        public bool RedirectStandardInput
        {
            get
            {
                return this.redirectStandardInput;
            }
            set
            {
                this.redirectStandardInput = value;
            }
        }

        [MonitoringDescription("ProcessRedirectStandardOutput"), DefaultValue(false), NotifyParentProperty(true)]
        public bool RedirectStandardOutput
        {
            get
            {
                return this.redirectStandardOutput;
            }
            set
            {
                this.redirectStandardOutput = value;
            }
        }

        public Encoding StandardErrorEncoding
        {
            get
            {
                return this.standardErrorEncoding;
            }
            set
            {
                this.standardErrorEncoding = value;
            }
        }

        public Encoding StandardOutputEncoding
        {
            get
            {
                return this.standardOutputEncoding;
            }
            set
            {
                this.standardOutputEncoding = value;
            }
        }

        [NotifyParentProperty(true)]
        public string UserName
        {
            get
            {
                if (this.userName == null)
                {
                    return string.Empty;
                }
                return this.userName;
            }
            set
            {
                this.userName = value;
            }
        }

        [DefaultValue(true), MonitoringDescription("ProcessUseShellExecute"), NotifyParentProperty(true)]
        public bool UseShellExecute
        {
            get
            {
                return this.useShellExecute;
            }
            set
            {
                this.useShellExecute = value;
            }
        }

        [NotifyParentProperty(true), DefaultValue(""), TypeConverter("System.Diagnostics.Design.VerbConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MonitoringDescription("ProcessVerb")]
        public string Verb
        {
            get
            {
                if (this.verb == null)
                {
                    return string.Empty;
                }
                return this.verb;
            }
            set
            {
                this.verb = value;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] Verbs
        {
            get
            {
                ArrayList list = new ArrayList();
                RegistryKey key = null;
                string extension = Path.GetExtension(this.FileName);
                try
                {
                    if ((extension != null) && (extension.Length > 0))
                    {
                        key = Registry.ClassesRoot.OpenSubKey(extension);
                        if (key != null)
                        {
                            string str2 = (string) key.GetValue(string.Empty);
                            key.Close();
                            key = Registry.ClassesRoot.OpenSubKey(str2 + @"\shell");
                            if (key != null)
                            {
                                string[] subKeyNames = key.GetSubKeyNames();
                                for (int i = 0; i < subKeyNames.Length; i++)
                                {
                                    if (string.Compare(subKeyNames[i], "new", StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        list.Add(subKeyNames[i]);
                                    }
                                }
                                key.Close();
                                key = null;
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
                }
                string[] array = new string[list.Count];
                list.CopyTo(array, 0);
                return array;
            }
        }

        [NotifyParentProperty(true), DefaultValue(0), MonitoringDescription("ProcessWindowStyle")]
        public ProcessWindowStyle WindowStyle
        {
            get
            {
                return this.windowStyle;
            }
            set
            {
                if (!Enum.IsDefined(typeof(ProcessWindowStyle), value))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ProcessWindowStyle));
                }
                this.windowStyle = value;
            }
        }

        [DefaultValue(""), NotifyParentProperty(true), TypeConverter("System.Diagnostics.Design.StringValueConverter, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), MonitoringDescription("ProcessWorkingDirectory"), Editor("System.Diagnostics.Design.WorkingDirectoryEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), SettingsBindable(true)]
        public string WorkingDirectory
        {
            get
            {
                if (this.directory == null)
                {
                    return string.Empty;
                }
                return this.directory;
            }
            set
            {
                this.directory = value;
            }
        }
    }
}

