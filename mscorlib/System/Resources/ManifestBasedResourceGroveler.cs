namespace System.Resources
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Eventing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal class ManifestBasedResourceGroveler : IResourceGroveler
    {
        private ResourceManager.ResourceManagerMediator _mediator;

        public ManifestBasedResourceGroveler(ResourceManager.ResourceManagerMediator mediator)
        {
            this._mediator = mediator;
        }

        private bool CanUseDefaultResourceClasses(string readerTypeName, string resSetTypeName)
        {
            if (this._mediator.UserResourceSet != null)
            {
                return false;
            }
            AssemblyName name = new AssemblyName(ResourceManager.MscorlibName);
            if ((readerTypeName != null) && !ResourceManager.CompareNames(readerTypeName, ResourceManager.ResReaderTypeName, name))
            {
                return false;
            }
            if ((resSetTypeName != null) && !ResourceManager.CompareNames(resSetTypeName, ResourceManager.ResSetTypeName, name))
            {
                return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private Stream CaseInsensitiveManifestResourceStreamLookup(RuntimeAssembly satellite, string name)
        {
            StringBuilder builder = new StringBuilder();
            if (this._mediator.LocationInfo != null)
            {
                string str = this._mediator.LocationInfo.Namespace;
                if (str != null)
                {
                    builder.Append(str);
                    if (name != null)
                    {
                        builder.Append(Type.Delimiter);
                    }
                }
            }
            builder.Append(name);
            string str2 = builder.ToString();
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            string str3 = null;
            foreach (string str4 in satellite.GetManifestResourceNames())
            {
                if (compareInfo.Compare(str4, str2, CompareOptions.IgnoreCase) == 0)
                {
                    if (str3 != null)
                    {
                        throw new MissingManifestResourceException(Environment.GetResourceString("MissingManifestResource_MultipleBlobs", new object[] { str2, satellite.ToString() }));
                    }
                    str3 = str4;
                }
            }
            if (FrameworkEventSource.IsInitialized)
            {
                if (str3 != null)
                {
                    FrameworkEventSource.Log.ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(this._mediator.BaseName, this._mediator.MainAssembly, satellite.GetSimpleName(), str2);
                }
                else
                {
                    FrameworkEventSource.Log.ResourceManagerCaseInsensitiveResourceStreamLookupFailed(this._mediator.BaseName, this._mediator.MainAssembly, satellite.GetSimpleName(), str2);
                }
            }
            if (str3 == null)
            {
                return null;
            }
            bool skipSecurityCheck = (this._mediator.MainAssembly == satellite) && (this._mediator.CallingAssembly == this._mediator.MainAssembly);
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            Stream stream = satellite.GetManifestResourceStream(str3, ref lookForMyCaller, skipSecurityCheck);
            if ((stream != null) && FrameworkEventSource.IsInitialized)
            {
                FrameworkEventSource.Log.ResourceManagerManifestResourceAccessDenied(this._mediator.BaseName, this._mediator.MainAssembly, satellite.GetSimpleName(), str3);
            }
            return stream;
        }

        [SecurityCritical]
        internal ResourceSet CreateResourceSet(Stream store, Assembly assembly)
        {
            ResourceSet set4;
            if (store.CanSeek && (store.Length > 4L))
            {
                long position = store.Position;
                BinaryReader reader = new BinaryReader(store);
                if (reader.ReadInt32() == ResourceManager.MagicNumber)
                {
                    Type userResourceSet;
                    int num3 = reader.ReadInt32();
                    string readerTypeName = null;
                    string resSetTypeName = null;
                    if (num3 == ResourceManager.HeaderVersionNumber)
                    {
                        reader.ReadInt32();
                        readerTypeName = reader.ReadString();
                        resSetTypeName = reader.ReadString();
                    }
                    else
                    {
                        if (num3 <= ResourceManager.HeaderVersionNumber)
                        {
                            throw new NotSupportedException(Environment.GetResourceString("NotSupported_ObsoleteResourcesFile", new object[] { this._mediator.MainAssembly.GetSimpleName() }));
                        }
                        int num4 = reader.ReadInt32();
                        long offset = reader.BaseStream.Position + num4;
                        readerTypeName = reader.ReadString();
                        resSetTypeName = reader.ReadString();
                        reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                    }
                    store.Position = position;
                    if (this.CanUseDefaultResourceClasses(readerTypeName, resSetTypeName))
                    {
                        return new RuntimeResourceSet(store);
                    }
                    Type type = Type.GetType(readerTypeName, true);
                    object[] objArray = new object[] { store };
                    IResourceReader reader2 = (IResourceReader) Activator.CreateInstance(type, objArray);
                    object[] objArray2 = new object[] { reader2 };
                    if (this._mediator.UserResourceSet == null)
                    {
                        userResourceSet = Type.GetType(resSetTypeName, true, false);
                    }
                    else
                    {
                        userResourceSet = this._mediator.UserResourceSet;
                    }
                    return (ResourceSet) Activator.CreateInstance(userResourceSet, BindingFlags.CreateInstance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, objArray2, null, null);
                }
                store.Position = position;
            }
            if (this._mediator.UserResourceSet == null)
            {
                return new RuntimeResourceSet(store);
            }
            object[] args = new object[] { store, assembly };
            try
            {
                ResourceSet set3 = null;
                try
                {
                    return (ResourceSet) Activator.CreateInstance(this._mediator.UserResourceSet, args);
                }
                catch (MissingMethodException)
                {
                }
                args = new object[] { store };
                set3 = (ResourceSet) Activator.CreateInstance(this._mediator.UserResourceSet, args);
                set4 = set3;
            }
            catch (MissingMethodException exception)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_ResMgrBadResSet_Type", new object[] { this._mediator.UserResourceSet.AssemblyQualifiedName }), exception);
            }
            return set4;
        }

        [SecurityCritical]
        private Stream GetManifestResourceStream(RuntimeAssembly satellite, string fileName, ref StackCrawlMark stackMark)
        {
            bool skipSecurityCheck = (this._mediator.MainAssembly == satellite) && (this._mediator.CallingAssembly == this._mediator.MainAssembly);
            Stream stream = satellite.GetManifestResourceStream(this._mediator.LocationInfo, fileName, skipSecurityCheck, ref stackMark);
            if (stream == null)
            {
                stream = this.CaseInsensitiveManifestResourceStreamLookup(satellite, fileName);
            }
            return stream;
        }

        [SecurityCritical]
        internal static CultureInfo GetNeutralResourcesLanguage(Assembly a, ref UltimateResourceFallbackLocation fallbackLocation)
        {
            IList<CustomAttributeData> customAttributes = CustomAttributeData.GetCustomAttributes(a);
            CustomAttributeData data = null;
            for (int i = 0; i < customAttributes.Count; i++)
            {
                if (customAttributes[i].Constructor.DeclaringType == typeof(NeutralResourcesLanguageAttribute))
                {
                    data = customAttributes[i];
                    break;
                }
            }
            if (data == null)
            {
                if (FrameworkEventSource.IsInitialized)
                {
                    FrameworkEventSource.Log.ResourceManagerNeutralResourceAttributeMissing(a);
                }
                fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
                return CultureInfo.InvariantCulture;
            }
            string name = null;
            if (data.Constructor.GetParameters().Length == 2)
            {
                CustomAttributeTypedArgument argument = data.ConstructorArguments[1];
                fallbackLocation = (UltimateResourceFallbackLocation) argument.Value;
                if ((fallbackLocation < UltimateResourceFallbackLocation.MainAssembly) || (fallbackLocation > UltimateResourceFallbackLocation.Satellite))
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidNeutralResourcesLanguage_FallbackLoc", new object[] { (UltimateResourceFallbackLocation) fallbackLocation }));
                }
            }
            else
            {
                fallbackLocation = UltimateResourceFallbackLocation.MainAssembly;
            }
            CustomAttributeTypedArgument argument2 = data.ConstructorArguments[0];
            name = argument2.Value as string;
            try
            {
                return CultureInfo.GetCultureInfo(name);
            }
            catch (ArgumentException exception)
            {
                if (a != typeof(object).Assembly)
                {
                    throw new ArgumentException(Environment.GetResourceString("Arg_InvalidNeutralResourcesLanguage_Asm_Culture", new object[] { a.ToString(), name }), exception);
                }
                return CultureInfo.InvariantCulture;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private RuntimeAssembly GetSatelliteAssembly(CultureInfo lookForCulture, ref StackCrawlMark stackMark)
        {
            if (!this._mediator.LookedForSatelliteContractVersion)
            {
                this._mediator.SatelliteContractVersion = this._mediator.ObtainSatelliteContractVersion(this._mediator.MainAssembly);
                this._mediator.LookedForSatelliteContractVersion = true;
            }
            RuntimeAssembly assembly = null;
            string satelliteAssemblyName = this.GetSatelliteAssemblyName();
            try
            {
                assembly = this._mediator.MainAssembly.InternalGetSatelliteAssembly(satelliteAssemblyName, lookForCulture, this._mediator.SatelliteContractVersion, false, ref stackMark);
            }
            catch (FileLoadException exception)
            {
                Win32Native.MakeHRFromErrorCode(5);
                int num1 = exception._HResult;
            }
            catch (BadImageFormatException)
            {
            }
            if (FrameworkEventSource.IsInitialized)
            {
                if (assembly != null)
                {
                    FrameworkEventSource.Log.ResourceManagerGetSatelliteAssemblySucceeded(this._mediator.BaseName, this._mediator.MainAssembly, lookForCulture.Name, satelliteAssemblyName);
                    return assembly;
                }
                FrameworkEventSource.Log.ResourceManagerGetSatelliteAssemblyFailed(this._mediator.BaseName, this._mediator.MainAssembly, lookForCulture.Name, satelliteAssemblyName);
            }
            return assembly;
        }

        [SecurityCritical]
        private string GetSatelliteAssemblyName()
        {
            return (this._mediator.MainAssembly.GetSimpleName() + ".resources");
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecuritySafeCritical]
        public ResourceSet GrovelForResourceSet(CultureInfo culture, Dictionary<string, ResourceSet> localResourceSets, bool tryParents, bool createIfNotExists, ref StackCrawlMark stackMark)
        {
            ResourceSet set = null;
            Stream store = null;
            RuntimeAssembly satellite = null;
            CultureInfo lookForCulture = this.UltimateFallbackFixup(culture);
            if (lookForCulture.HasInvariantCultureName && (this._mediator.FallbackLoc == UltimateResourceFallbackLocation.MainAssembly))
            {
                satellite = this._mediator.MainAssembly;
            }
            else if (!lookForCulture.HasInvariantCultureName && !this._mediator.TryLookingForSatellite(lookForCulture))
            {
                satellite = null;
            }
            else
            {
                satellite = this.GetSatelliteAssembly(lookForCulture, ref stackMark);
                if ((satellite == null) && (culture.HasInvariantCultureName && (this._mediator.FallbackLoc == UltimateResourceFallbackLocation.Satellite)))
                {
                    this.HandleSatelliteMissing();
                }
            }
            string resourceFileName = this._mediator.GetResourceFileName(lookForCulture);
            if (satellite != null)
            {
                lock (localResourceSets)
                {
                    if (localResourceSets.TryGetValue(culture.Name, out set) && FrameworkEventSource.IsInitialized)
                    {
                        FrameworkEventSource.Log.ResourceManagerFoundResourceSetInCacheUnexpected(this._mediator.BaseName, this._mediator.MainAssembly, culture.Name);
                    }
                }
                store = this.GetManifestResourceStream(satellite, resourceFileName, ref stackMark);
            }
            if (FrameworkEventSource.IsInitialized)
            {
                if (store != null)
                {
                    FrameworkEventSource.Log.ResourceManagerStreamFound(this._mediator.BaseName, this._mediator.MainAssembly, culture.Name, satellite, resourceFileName);
                }
                else
                {
                    FrameworkEventSource.Log.ResourceManagerStreamNotFound(this._mediator.BaseName, this._mediator.MainAssembly, culture.Name, satellite, resourceFileName);
                }
            }
            if ((createIfNotExists && (store != null)) && (set == null))
            {
                if (FrameworkEventSource.IsInitialized)
                {
                    FrameworkEventSource.Log.ResourceManagerCreatingResourceSet(this._mediator.BaseName, this._mediator.MainAssembly, culture.Name, resourceFileName);
                }
                set = this.CreateResourceSet(store, satellite);
            }
            else if (((store == null) && tryParents) && culture.HasInvariantCultureName)
            {
                this.HandleResourceStreamMissing(resourceFileName);
            }
            if ((!createIfNotExists && (store != null)) && ((set == null) && FrameworkEventSource.IsInitialized))
            {
                FrameworkEventSource.Log.ResourceManagerNotCreatingResourceSet(this._mediator.BaseName, this._mediator.MainAssembly, culture.Name);
            }
            return set;
        }

        [SecurityCritical]
        private void HandleResourceStreamMissing(string fileName)
        {
            if ((this._mediator.MainAssembly == typeof(object).Assembly) && this._mediator.BaseName.Equals("mscorlib"))
            {
                string message = "mscorlib.resources couldn't be found!  Large parts of the BCL won't work!";
                Environment.FailFast(message);
            }
            string str2 = string.Empty;
            if ((this._mediator.LocationInfo != null) && (this._mediator.LocationInfo.Namespace != null))
            {
                str2 = this._mediator.LocationInfo.Namespace + Type.Delimiter;
            }
            str2 = str2 + fileName;
            throw new MissingManifestResourceException(Environment.GetResourceString("MissingManifestResource_NoNeutralAsm", new object[] { str2, this._mediator.MainAssembly.GetSimpleName() }));
        }

        [SecurityCritical]
        private void HandleSatelliteMissing()
        {
            string str = this._mediator.MainAssembly.GetSimpleName() + ".resources.dll";
            if (this._mediator.SatelliteContractVersion != null)
            {
                str = str + ", Version=" + this._mediator.SatelliteContractVersion.ToString();
            }
            AssemblyName name = new AssemblyName();
            name.SetPublicKey(this._mediator.MainAssembly.GetPublicKey());
            byte[] publicKeyToken = name.GetPublicKeyToken();
            int length = publicKeyToken.Length;
            StringBuilder builder = new StringBuilder(length * 2);
            for (int i = 0; i < length; i++)
            {
                builder.Append(publicKeyToken[i].ToString("x", CultureInfo.InvariantCulture));
            }
            str = str + ", PublicKeyToken=" + builder;
            string cultureName = this._mediator.NeutralResourcesCulture.Name;
            if (cultureName.Length == 0)
            {
                cultureName = "<invariant>";
            }
            throw new MissingSatelliteAssemblyException(Environment.GetResourceString("MissingSatelliteAssembly_Culture_Name", new object[] { this._mediator.NeutralResourcesCulture, str }), cultureName);
        }

        public bool HasNeutralResources(CultureInfo culture, string defaultResName)
        {
            string str = defaultResName;
            if ((this._mediator.LocationInfo != null) && (this._mediator.LocationInfo.Namespace != null))
            {
                str = this._mediator.LocationInfo.Namespace + Type.Delimiter + defaultResName;
            }
            foreach (string str2 in this._mediator.MainAssembly.GetManifestResourceNames())
            {
                if (str2.Equals(str))
                {
                    return true;
                }
            }
            return false;
        }

        private CultureInfo UltimateFallbackFixup(CultureInfo lookForCulture)
        {
            CultureInfo neutralResourcesCulture = lookForCulture;
            if ((lookForCulture.Name == this._mediator.NeutralResourcesCulture.Name) && (this._mediator.FallbackLoc == UltimateResourceFallbackLocation.MainAssembly))
            {
                if (FrameworkEventSource.IsInitialized)
                {
                    FrameworkEventSource.Log.ResourceManagerNeutralResourcesSufficient(this._mediator.BaseName, this._mediator.MainAssembly, lookForCulture.Name);
                }
                return CultureInfo.InvariantCulture;
            }
            if (lookForCulture.HasInvariantCultureName && (this._mediator.FallbackLoc == UltimateResourceFallbackLocation.Satellite))
            {
                neutralResourcesCulture = this._mediator.NeutralResourcesCulture;
            }
            return neutralResourcesCulture;
        }
    }
}

