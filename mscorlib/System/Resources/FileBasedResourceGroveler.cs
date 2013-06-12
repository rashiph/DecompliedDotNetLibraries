namespace System.Resources
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal class FileBasedResourceGroveler : IResourceGroveler
    {
        private ResourceManager.ResourceManagerMediator _mediator;

        public FileBasedResourceGroveler(ResourceManager.ResourceManagerMediator mediator)
        {
            this._mediator = mediator;
        }

        [SecurityCritical]
        private ResourceSet CreateResourceSet(string file)
        {
            ResourceSet set;
            if (this._mediator.UserResourceSet == null)
            {
                return new RuntimeResourceSet(file);
            }
            object[] args = new object[] { file };
            try
            {
                set = (ResourceSet) Activator.CreateInstance(this._mediator.UserResourceSet, args);
            }
            catch (MissingMethodException exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResMgrBadResSet_Type", new object[] { this._mediator.UserResourceSet.AssemblyQualifiedName }), exception);
            }
            return set;
        }

        private string FindResourceFile(CultureInfo culture, string fileName)
        {
            if (this._mediator.ModuleDir != null)
            {
                string path = Path.Combine(this._mediator.ModuleDir, fileName);
                if (File.Exists(path))
                {
                    return path;
                }
            }
            if (File.Exists(fileName))
            {
                return fileName;
            }
            return null;
        }

        [SecuritySafeCritical]
        public ResourceSet GrovelForResourceSet(CultureInfo culture, Dictionary<string, ResourceSet> localResourceSets, bool tryParents, bool createIfNotExists, ref StackCrawlMark stackMark)
        {
            string file = null;
            ResourceSet set = null;
            ResourceSet set2;
            try
            {
                new FileIOPermission(PermissionState.Unrestricted).Assert();
                string resourceFileName = this._mediator.GetResourceFileName(culture);
                file = this.FindResourceFile(culture, resourceFileName);
                if (file == null)
                {
                    if (tryParents && culture.HasInvariantCultureName)
                    {
                        throw new MissingManifestResourceException(Environment.GetResourceString("MissingManifestResource_NoNeutralDisk") + Environment.NewLine + "baseName: " + this._mediator.BaseNameField + "  locationInfo: " + ((this._mediator.LocationInfo == null) ? "<null>" : this._mediator.LocationInfo.FullName) + "  fileName: " + this._mediator.GetResourceFileName(culture));
                    }
                }
                else
                {
                    set = this.CreateResourceSet(file);
                }
                set2 = set;
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return set2;
        }

        public bool HasNeutralResources(CultureInfo culture, string defaultResName)
        {
            string path = this.FindResourceFile(culture, defaultResName);
            if ((path != null) && File.Exists(path))
            {
                return true;
            }
            string moduleDir = this._mediator.ModuleDir;
            if (path != null)
            {
                Path.GetDirectoryName(path);
            }
            return false;
        }
    }
}

