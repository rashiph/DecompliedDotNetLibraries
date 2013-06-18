namespace System.Workflow.ComponentModel.Design
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Text;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Xml;

    public sealed class WorkflowTheme : IDisposable
    {
        private static WorkflowTheme currentTheme = null;
        private static Font defaultFont = null;
        internal static string DefaultNamespace = typeof(WorkflowTheme).Namespace.Replace(".", "_");
        internal const string DefaultThemeFileExtension = "*.wtm";
        private string description = DR.GetString("DefaultThemeDescription", new object[0]);
        private ThemeCollection designerThemes = new ThemeCollection();
        private static bool enableChangeNotification = true;
        private string filePath = string.Empty;
        private string name = string.Empty;
        private bool readOnly;
        private const string ThemePathKey = "ThemeFilePath";
        private const string ThemeResourceNS = "System.Workflow.ComponentModel.Design.ActivityDesignerThemes.";
        private ThemeType themeType = ThemeType.UserDefined;
        private const string ThemeTypeKey = "ThemeType";
        private static IUIService uiService = null;
        private string version = "1.0";
        private static readonly string WorkflowThemesSubKey = "Themes";

        public static  event EventHandler ThemeChanged;

        static WorkflowTheme()
        {
            currentTheme = LoadThemeSettingFromRegistry();
            if (currentTheme != null)
            {
                currentTheme.ReadOnly = true;
            }
        }

        public WorkflowTheme()
        {
            this.filePath = GenerateThemeFilePath();
            if ((this.filePath != null) && (this.filePath.Length > 0))
            {
                this.name = Path.GetFileNameWithoutExtension(this.filePath);
            }
        }

        internal void AmbientPropertyChanged(System.Workflow.ComponentModel.Design.AmbientProperty ambientProperty)
        {
            foreach (DesignerTheme theme in this.designerThemes)
            {
                bool readOnly = this.ReadOnly;
                this.ReadOnly = false;
                theme.OnAmbientPropertyChanged(ambientProperty);
                this.ReadOnly = readOnly;
            }
        }

        public WorkflowTheme Clone()
        {
            WorkflowTheme theme = null;
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            using (serializationManager.CreateSession())
            {
                ThemeSerializationProvider provider = new ThemeSerializationProvider();
                StringWriter output = new StringWriter(new StringBuilder(), CultureInfo.InvariantCulture);
                StringReader input = null;
                try
                {
                    ((IDesignerSerializationManager) serializationManager).AddSerializationProvider(provider);
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    using (XmlWriter writer2 = Helpers.CreateXmlWriter(output))
                    {
                        serializer.Serialize(serializationManager, writer2, this);
                    }
                    input = new StringReader(output.ToString());
                    using (XmlReader reader2 = XmlReader.Create(input))
                    {
                        theme = serializer.Deserialize(serializationManager, reader2) as WorkflowTheme;
                    }
                }
                finally
                {
                    ((IDesignerSerializationManager) serializationManager).RemoveSerializationProvider(provider);
                    input.Close();
                    output.Close();
                }
            }
            if (theme != null)
            {
                theme.filePath = this.filePath;
                foreach (DesignerTheme theme2 in theme.DesignerThemes)
                {
                    theme2.Initialize();
                }
            }
            return theme;
        }

        public static WorkflowTheme CreateStandardTheme(ThemeType standardThemeType)
        {
            WorkflowTheme theme = null;
            if (standardThemeType == ThemeType.Default)
            {
                theme = new WorkflowTheme();
                theme.AmbientTheme.UseDefaultFont();
            }
            else if (standardThemeType == ThemeType.System)
            {
                theme = new WorkflowTheme {
                    AmbientTheme = { UseOperatingSystemSettings = true }
                };
            }
            else
            {
                return null;
            }
            string[] strArray = StandardThemes[standardThemeType];
            if (strArray != null)
            {
                theme.Name = strArray[0];
                theme.themeType = standardThemeType;
                theme.Description = strArray[1];
                theme.FilePath = LookupPath;
            }
            return theme;
        }

        private void Dispose(bool disposing)
        {
            foreach (DesignerTheme theme in this.designerThemes)
            {
                ((IDisposable) theme).Dispose();
            }
            this.designerThemes.Clear();
        }

        ~WorkflowTheme()
        {
            this.Dispose(false);
        }

        internal static void FireThemeChange()
        {
            if (ThemeChanged != null)
            {
                ThemeChanged(currentTheme, EventArgs.Empty);
            }
        }

        public static string GenerateThemeFilePath()
        {
            string lookupPath = LookupPath;
            string path = Path.Combine(lookupPath, DR.GetString("MyFavoriteTheme", new object[0]) + ".wtm");
            for (int i = 1; File.Exists(path); i++)
            {
                path = Path.Combine(lookupPath, DR.GetString("MyFavoriteTheme", new object[0]) + i.ToString(CultureInfo.InvariantCulture) + ".wtm");
            }
            return path;
        }

        internal static Font GetDefaultFont()
        {
            if (defaultFont == null)
            {
                if (UIService != null)
                {
                    defaultFont = UIService.Styles["DialogFont"] as Font;
                }
                if (defaultFont == null)
                {
                    defaultFont = Control.DefaultFont;
                }
            }
            return defaultFont;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ActivityDesignerTheme GetDesignerTheme(ActivityDesigner designer)
        {
            if (designer == null)
            {
                throw new ArgumentNullException("designer");
            }
            return (this.GetTheme(designer.GetType()) as ActivityDesignerTheme);
        }

        internal DesignerTheme GetTheme(System.Type designerType)
        {
            bool readOnly = this.ReadOnly;
            DesignerTheme item = this.designerThemes.Contains(designerType.FullName) ? this.designerThemes[designerType.FullName] : null;
            try
            {
                this.ReadOnly = false;
                if ((item == null) || ((item.DesignerType != null) && !designerType.Equals(item.DesignerType)))
                {
                    bool flag2 = item != null;
                    ActivityDesignerThemeAttribute attribute = TypeDescriptor.GetAttributes(designerType)[typeof(ActivityDesignerThemeAttribute)] as ActivityDesignerThemeAttribute;
                    if (attribute == null)
                    {
                        throw new InvalidOperationException(DR.GetString("Error_ThemeAttributeMissing", new object[] { designerType.FullName }));
                    }
                    if (attribute.DesignerThemeType == null)
                    {
                        throw new InvalidOperationException(DR.GetString("Error_ThemeTypeMissing", new object[] { designerType.FullName }));
                    }
                    if (attribute.Xml.Length > 0)
                    {
                        Stream manifestResourceStream = designerType.Assembly.GetManifestResourceStream(designerType, attribute.Xml);
                        if (manifestResourceStream == null)
                        {
                            manifestResourceStream = designerType.Assembly.GetManifestResourceStream("System.Workflow.ComponentModel.Design.ActivityDesignerThemes." + attribute.Xml);
                        }
                        XmlReader reader = (manifestResourceStream != null) ? XmlReader.Create(manifestResourceStream) : null;
                        if (reader == null)
                        {
                            reader = XmlReader.Create(new StringReader(attribute.Xml));
                        }
                        if (reader != null)
                        {
                            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
                            using (serializationManager.CreateSession())
                            {
                                ThemeSerializationProvider provider = new ThemeSerializationProvider();
                                try
                                {
                                    ((IDesignerSerializationManager) serializationManager).AddSerializationProvider(provider);
                                    ((IDesignerSerializationManager) serializationManager).Context.Push(this);
                                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                                    item = serializer.Deserialize(serializationManager, reader) as DesignerTheme;
                                    if ((item != null) && !attribute.DesignerThemeType.IsAssignableFrom(item.GetType()))
                                    {
                                        ((IDesignerSerializationManager) serializationManager).ReportError(new WorkflowMarkupSerializationException(DR.GetString("ThemeTypesMismatch", new object[] { attribute.DesignerThemeType.FullName, item.GetType().FullName })));
                                        item = null;
                                    }
                                    if (serializationManager.Errors.Count > 0)
                                    {
                                        string str = string.Empty;
                                        foreach (object obj2 in serializationManager.Errors)
                                        {
                                            str = str + obj2.ToString() + @"\n";
                                        }
                                    }
                                }
                                finally
                                {
                                    ((IDesignerSerializationManager) serializationManager).RemoveSerializationProvider(provider);
                                    reader.Close();
                                }
                            }
                        }
                    }
                    if (item == null)
                    {
                        try
                        {
                            item = Activator.CreateInstance(attribute.DesignerThemeType, new object[] { this }) as DesignerTheme;
                        }
                        catch
                        {
                            item = new ActivityDesignerTheme(this);
                        }
                    }
                    item.DesignerType = designerType;
                    item.ApplyTo = designerType.FullName;
                    item.Initialize();
                    if (flag2)
                    {
                        this.designerThemes.Remove(designerType.FullName);
                    }
                    this.designerThemes.Add(item);
                }
                if (item.DesignerType == null)
                {
                    item.DesignerType = designerType;
                }
            }
            finally
            {
                this.ReadOnly = readOnly;
            }
            return item;
        }

        public static WorkflowTheme Load(string themeFilePath)
        {
            DesignerSerializationManager serializationManager = new DesignerSerializationManager();
            using (serializationManager.CreateSession())
            {
                return Load(serializationManager, themeFilePath);
            }
        }

        public static WorkflowTheme Load(IDesignerSerializationManager serializationManager, string themeFilePath)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            WorkflowTheme theme = null;
            if ((themeFilePath != null) && File.Exists(themeFilePath))
            {
                XmlReader reader = XmlReader.Create(themeFilePath);
                ThemeSerializationProvider provider = new ThemeSerializationProvider();
                try
                {
                    serializationManager.AddSerializationProvider(provider);
                    WorkflowMarkupSerializer serializer = new WorkflowMarkupSerializer();
                    theme = serializer.Deserialize(serializationManager, reader) as WorkflowTheme;
                }
                finally
                {
                    serializationManager.RemoveSerializationProvider(provider);
                    reader.Close();
                }
                if (theme != null)
                {
                    theme.filePath = themeFilePath;
                }
            }
            return theme;
        }

        public static WorkflowTheme LoadThemeSettingFromRegistry()
        {
            WorkflowTheme theme = null;
            RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
            if (key != null)
            {
                ThemeType standardThemeType = ThemeType.Default;
                try
                {
                    object obj2 = key.GetValue("ThemeType");
                    if (obj2 is string)
                    {
                        standardThemeType = (ThemeType) Enum.Parse(typeof(ThemeType), (string) obj2, true);
                    }
                    if (standardThemeType == ThemeType.UserDefined)
                    {
                        obj2 = key.GetValue("ThemeFilePath");
                        string path = (obj2 is string) ? ((string) obj2) : string.Empty;
                        if (File.Exists(path) && Path.GetExtension(path).Equals("*.wtm".Replace("*", ""), StringComparison.Ordinal))
                        {
                            theme = Load(path);
                        }
                    }
                }
                catch
                {
                }
                finally
                {
                    if (theme == null)
                    {
                        if (standardThemeType == ThemeType.UserDefined)
                        {
                            standardThemeType = ThemeType.Default;
                        }
                        theme = CreateStandardTheme(standardThemeType);
                    }
                    key.Close();
                }
            }
            return theme;
        }

        public void Save(string themeFilePath)
        {
            if ((themeFilePath == null) || (themeFilePath.Length == 0))
            {
                throw new ArgumentException(DR.GetString("ThemePathNotValid", new object[0]), "themeFilePath");
            }
            DesignerSerializationManager manager = new DesignerSerializationManager();
            using (manager.CreateSession())
            {
                WorkflowMarkupSerializationManager serializationManager = new WorkflowMarkupSerializationManager(manager);
                XmlWriter writer = null;
                ThemeSerializationProvider provider = new ThemeSerializationProvider();
                try
                {
                    string directoryName = Path.GetDirectoryName(themeFilePath);
                    if ((directoryName.Length > 0) && !Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }
                    writer = Helpers.CreateXmlWriter(themeFilePath);
                    serializationManager.AddSerializationProvider(provider);
                    new WorkflowMarkupSerializer().Serialize(serializationManager, writer, this);
                }
                finally
                {
                    serializationManager.RemoveSerializationProvider(provider);
                    if (writer != null)
                    {
                        writer.Close();
                    }
                }
                this.filePath = themeFilePath;
            }
        }

        public static void SaveThemeSettingToRegistry()
        {
            RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
            if (key != null)
            {
                try
                {
                    key.SetValue("ThemeType", CurrentTheme.themeType);
                    if (CurrentTheme.themeType == ThemeType.UserDefined)
                    {
                        key.SetValue("ThemeFilePath", CurrentTheme.FilePath);
                    }
                    else
                    {
                        key.SetValue("ThemeFilePath", string.Empty);
                    }
                }
                catch
                {
                }
                finally
                {
                    key.Close();
                }
            }
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public System.Workflow.ComponentModel.Design.AmbientTheme AmbientTheme
        {
            get
            {
                return (this.GetTheme(typeof(WorkflowView)) as System.Workflow.ComponentModel.Design.AmbientTheme);
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ContainingFileDirectory
        {
            get
            {
                string str = string.Empty;
                if (this.filePath.Length > 0)
                {
                    try
                    {
                        str = Path.GetDirectoryName(this.filePath) + Path.DirectorySeparatorChar;
                    }
                    catch
                    {
                    }
                }
                return str;
            }
        }

        public static WorkflowTheme CurrentTheme
        {
            get
            {
                if (currentTheme == null)
                {
                    currentTheme = CreateStandardTheme(ThemeType.Default);
                    currentTheme.ReadOnly = true;
                }
                return currentTheme;
            }
            set
            {
                if (WorkflowTheme.currentTheme != value)
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException("value");
                    }
                    WorkflowTheme currentTheme = WorkflowTheme.currentTheme;
                    WorkflowTheme.currentTheme = value;
                    WorkflowTheme.currentTheme.ReadOnly = true;
                    if (EnableChangeNotification)
                    {
                        if (currentTheme != null)
                        {
                            ((IDisposable) currentTheme).Dispose();
                            currentTheme = null;
                        }
                        FireThemeChange();
                    }
                }
            }
        }

        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.description;
            }
            set
            {
                if (this.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.description = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList DesignerThemes
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.designerThemes;
            }
        }

        public static bool EnableChangeNotification
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return enableChangeNotification;
            }
            set
            {
                if (enableChangeNotification != value)
                {
                    enableChangeNotification = value;
                }
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string FilePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.filePath;
            }
            set
            {
                if (this.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.filePath = value;
            }
        }

        public static string LookupPath
        {
            get
            {
                string folderPath = string.Empty;
                try
                {
                    folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (string.IsNullOrEmpty(folderPath))
                    {
                        folderPath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
                    }
                    folderPath = Path.Combine(folderPath, "Windows Workflow Foundation" + Path.DirectorySeparatorChar + "Themes");
                    folderPath = folderPath + Path.DirectorySeparatorChar;
                }
                catch
                {
                }
                return folderPath;
            }
        }

        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            set
            {
                if (this.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.name = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ReadOnly
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.readOnly;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.readOnly = value;
            }
        }

        public static string RegistryKeyPath
        {
            get
            {
                return (DesignerHelpers.DesignerPerUserRegistryKey + @"\" + WorkflowThemesSubKey);
            }
        }

        public static IDictionary<ThemeType, string[]> StandardThemes
        {
            get
            {
                Dictionary<ThemeType, string[]> dictionary = new Dictionary<ThemeType, string[]>();
                dictionary.Add(ThemeType.Default, new string[] { DR.GetString("DefaultTheme", new object[0]), DR.GetString("DefaultThemeDescription", new object[0]) });
                dictionary.Add(ThemeType.System, new string[] { DR.GetString("OSTheme", new object[0]), DR.GetString("SystemThemeDescription", new object[0]) });
                return dictionary;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ThemeType Type
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.themeType;
            }
        }

        internal static IUIService UIService
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return uiService;
            }
            set
            {
                uiService = value;
                defaultFont = null;
                CurrentTheme.AmbientTheme.UpdateFont();
            }
        }

        public string Version
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.version;
            }
            set
            {
                if (this.ReadOnly)
                {
                    throw new InvalidOperationException(DR.GetString("ThemePropertyReadOnly", new object[0]));
                }
                this.version = value;
            }
        }

        private class ThemeCollection : KeyedCollection<string, DesignerTheme>
        {
            protected override string GetKeyForItem(DesignerTheme item)
            {
                return item.ApplyTo;
            }
        }
    }
}

