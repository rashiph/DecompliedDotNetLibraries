namespace System.ComponentModel
{
    using System;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.IO;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    public class LicFileLicenseProvider : LicenseProvider
    {
        protected virtual string GetKey(Type type)
        {
            return string.Format(CultureInfo.InvariantCulture, "{0} is a licensed component.", new object[] { type.FullName });
        }

        public override License GetLicense(LicenseContext context, Type type, object instance, bool allowExceptions)
        {
            LicFileLicense license = null;
            if (context != null)
            {
                if (context.UsageMode == LicenseUsageMode.Runtime)
                {
                    string savedLicenseKey = context.GetSavedLicenseKey(type, null);
                    if ((savedLicenseKey != null) && this.IsKeyValid(savedLicenseKey, type))
                    {
                        license = new LicFileLicense(this, savedLicenseKey);
                    }
                }
                if (license != null)
                {
                    return license;
                }
                string path = null;
                if (context != null)
                {
                    ITypeResolutionService service = (ITypeResolutionService) context.GetService(typeof(ITypeResolutionService));
                    if (service != null)
                    {
                        path = service.GetPathOfAssembly(type.Assembly.GetName());
                    }
                }
                if (path == null)
                {
                    path = type.Module.FullyQualifiedName;
                }
                string str4 = Path.GetDirectoryName(path) + @"\" + type.FullName + ".lic";
                if (File.Exists(str4))
                {
                    Stream stream = new FileStream(str4, FileMode.Open, FileAccess.Read, FileShare.Read);
                    StreamReader reader = new StreamReader(stream);
                    string key = reader.ReadLine();
                    reader.Close();
                    if (this.IsKeyValid(key, type))
                    {
                        license = new LicFileLicense(this, this.GetKey(type));
                    }
                }
                if (license != null)
                {
                    context.SetSavedLicenseKey(type, license.LicenseKey);
                }
            }
            return license;
        }

        protected virtual bool IsKeyValid(string key, Type type)
        {
            return ((key != null) && key.StartsWith(this.GetKey(type)));
        }

        private class LicFileLicense : License
        {
            private string key;
            private LicFileLicenseProvider owner;

            public LicFileLicense(LicFileLicenseProvider owner, string key)
            {
                this.owner = owner;
                this.key = key;
            }

            public override void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public override string LicenseKey
            {
                get
                {
                    return this.key;
                }
            }
        }
    }
}

