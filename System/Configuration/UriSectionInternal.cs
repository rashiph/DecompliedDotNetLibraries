namespace System.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    internal sealed class UriSectionInternal
    {
        private static readonly object classSyncObject = new object();
        private UriIdnScope idnScope;
        private bool iriParsing;
        private Dictionary<string, SchemeSettingInternal> schemeSettings;

        private UriSectionInternal()
        {
            this.schemeSettings = new Dictionary<string, SchemeSettingInternal>();
        }

        private UriSectionInternal(UriSection section) : this()
        {
            this.idnScope = section.Idn.Enabled;
            this.iriParsing = section.IriParsing.Enabled;
            if (section.SchemeSettings != null)
            {
                foreach (SchemeSettingElement element in section.SchemeSettings)
                {
                    SchemeSettingInternal internal2 = new SchemeSettingInternal(element.Name, element.GenericUriParserOptions);
                    this.schemeSettings.Add(internal2.Name, internal2);
                }
            }
        }

        private UriSectionInternal(UriIdnScope idnScope, bool iriParsing, IEnumerable<SchemeSettingInternal> schemeSettings) : this()
        {
            this.idnScope = idnScope;
            this.iriParsing = iriParsing;
            if (schemeSettings != null)
            {
                foreach (SchemeSettingInternal internal2 in schemeSettings)
                {
                    this.schemeSettings.Add(internal2.Name, internal2);
                }
            }
        }

        internal SchemeSettingInternal GetSchemeSetting(string scheme)
        {
            SchemeSettingInternal internal2;
            if (this.schemeSettings.TryGetValue(scheme.ToLowerInvariant(), out internal2))
            {
                return internal2;
            }
            return null;
        }

        internal static UriSectionInternal GetSection()
        {
            lock (classSyncObject)
            {
                string appConfigFile = null;
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                try
                {
                    appConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                }
                finally
                {
                    CodeAccessPermission.RevertAssert();
                }
                if (IsWebConfig(appConfigFile))
                {
                    return LoadUsingSystemConfiguration();
                }
                return LoadUsingCustomParser(appConfigFile);
            }
        }

        private static bool IsWebConfig(string appConfigFile)
        {
            if (!(AppDomain.CurrentDomain.GetData(".appVPath") is string) && ((appConfigFile == null) || (!appConfigFile.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !appConfigFile.StartsWith("https://", StringComparison.OrdinalIgnoreCase))))
            {
                return false;
            }
            return true;
        }

        private static UriSectionInternal LoadUsingCustomParser(string appConfigFilePath)
        {
            string runtimeDirectory = null;
            new FileIOPermission(PermissionState.Unrestricted).Assert();
            try
            {
                runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            UriSectionData parentData = UriSectionReader.Read(Path.Combine(Path.Combine(runtimeDirectory, "Config"), "machine.config"));
            UriSectionData data2 = UriSectionReader.Read(appConfigFilePath, parentData);
            UriSectionData data3 = null;
            if (data2 != null)
            {
                data3 = data2;
            }
            else if (parentData != null)
            {
                data3 = parentData;
            }
            if (data3 != null)
            {
                UriIdnScope? idnScope = data3.IdnScope;
                UriIdnScope scope = idnScope.HasValue ? idnScope.GetValueOrDefault() : UriIdnScope.None;
                bool? iriParsing = data3.IriParsing;
                bool flag = iriParsing.HasValue ? iriParsing.GetValueOrDefault() : false;
                return new UriSectionInternal(scope, flag, data3.SchemeSettings.Values);
            }
            return null;
        }

        private static UriSectionInternal LoadUsingSystemConfiguration()
        {
            try
            {
                UriSection section = System.Configuration.PrivilegedConfigurationManager.GetSection("uri") as UriSection;
                if (section == null)
                {
                    return null;
                }
                return new UriSectionInternal(section);
            }
            catch (ConfigurationException)
            {
                return null;
            }
        }

        internal UriIdnScope IdnScope
        {
            get
            {
                return this.idnScope;
            }
        }

        internal bool IriParsing
        {
            get
            {
                return this.iriParsing;
            }
        }
    }
}

