namespace System.Diagnostics.Eventing
{
    using System;
    using System.Reflection;

    internal sealed class FrameworkEventSource : EventProviderBase
    {
        public const EventKeywords Loader = 1L;
        public static readonly FrameworkEventSource Log = new FrameworkEventSource();
        public const EventTask ResourceManagerTask = ((EventTask) 1);

        private FrameworkEventSource() : base(new Guid(0x8e9f5090, 0x2d75, 0x4d03, 0x8a, 0x81, 0xe5, 0xaf, 0xbf, 0x85, 0xda, 0xf1))
        {
        }

        private static string GetName(Assembly assembly)
        {
            if (assembly == null)
            {
                return "<<NULL>>";
            }
            return assembly.FullName;
        }

        [NonEvent]
        public void ResourceManagerAddingCultureFromConfigFile(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerAddingCultureFromConfigFile(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(20, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerAddingCultureFromConfigFile(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(20, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupFailed(string baseName, Assembly mainAssembly, string assemblyName, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerCaseInsensitiveResourceStreamLookupFailed(baseName, GetName(mainAssembly), assemblyName, resourceFileName);
            }
        }

        [Event(10, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupFailed(string baseName, string mainAssemblyName, string assemblyName, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(10, new object[] { baseName, mainAssemblyName, assemblyName, resourceFileName });
            }
        }

        [NonEvent]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(string baseName, Assembly mainAssembly, string assemblyName, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(baseName, GetName(mainAssembly), assemblyName, resourceFileName);
            }
        }

        [Event(9, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerCaseInsensitiveResourceStreamLookupSucceeded(string baseName, string mainAssemblyName, string assemblyName, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(9, new object[] { baseName, mainAssemblyName, assemblyName, resourceFileName });
            }
        }

        [NonEvent]
        public void ResourceManagerCreatingResourceSet(string baseName, Assembly mainAssembly, string cultureName, string fileName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerCreatingResourceSet(baseName, GetName(mainAssembly), cultureName, fileName);
            }
        }

        [Event(14, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerCreatingResourceSet(string baseName, string mainAssemblyName, string cultureName, string fileName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(14, new object[] { baseName, mainAssemblyName, cultureName, fileName });
            }
        }

        [NonEvent]
        public void ResourceManagerCultureFoundInConfigFile(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerCultureFoundInConfigFile(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(0x16, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerCultureFoundInConfigFile(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(0x16, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerCultureNotFoundInConfigFile(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerCultureNotFoundInConfigFile(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(0x15, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerCultureNotFoundInConfigFile(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(0x15, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerFoundResourceSetInCache(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerFoundResourceSetInCache(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(3, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerFoundResourceSetInCache(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(3, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerFoundResourceSetInCacheUnexpected(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerFoundResourceSetInCacheUnexpected(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(4, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerFoundResourceSetInCacheUnexpected(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(4, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerGetSatelliteAssemblyFailed(string baseName, Assembly mainAssembly, string cultureName, string assemblyName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerGetSatelliteAssemblyFailed(baseName, GetName(mainAssembly), cultureName, assemblyName);
            }
        }

        [Event(8, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerGetSatelliteAssemblyFailed(string baseName, string mainAssemblyName, string cultureName, string assemblyName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(8, new object[] { baseName, mainAssemblyName, cultureName, assemblyName });
            }
        }

        [NonEvent]
        public void ResourceManagerGetSatelliteAssemblySucceeded(string baseName, Assembly mainAssembly, string cultureName, string assemblyName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerGetSatelliteAssemblySucceeded(baseName, GetName(mainAssembly), cultureName, assemblyName);
            }
        }

        [Event(7, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerGetSatelliteAssemblySucceeded(string baseName, string mainAssemblyName, string cultureName, string assemblyName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(7, new object[] { baseName, mainAssemblyName, cultureName, assemblyName });
            }
        }

        [NonEvent]
        public void ResourceManagerLookingForResourceSet(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerLookingForResourceSet(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(2, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerLookingForResourceSet(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(2, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerLookupFailed(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerLookupFailed(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(0x10, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerLookupFailed(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(0x10, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerLookupStarted(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerLookupStarted(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(1, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerLookupStarted(string baseName, string mainAssemblyName, string cultureName)
        {
            base.WriteEvent(1, baseName, mainAssemblyName, cultureName);
        }

        [NonEvent]
        public void ResourceManagerManifestResourceAccessDenied(string baseName, Assembly mainAssembly, string assemblyName, string canonicalName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerManifestResourceAccessDenied(baseName, GetName(mainAssembly), assemblyName, canonicalName);
            }
        }

        [Event(11, Level=EventLevel.Error, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerManifestResourceAccessDenied(string baseName, string mainAssemblyName, string assemblyName, string canonicalName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(11, new object[] { baseName, mainAssemblyName, assemblyName, canonicalName });
            }
        }

        [NonEvent]
        public void ResourceManagerNeutralResourceAttributeMissing(Assembly mainAssembly)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerNeutralResourceAttributeMissing(GetName(mainAssembly));
            }
        }

        [Event(13, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerNeutralResourceAttributeMissing(string mainAssemblyName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(13, mainAssemblyName);
            }
        }

        [NonEvent]
        public void ResourceManagerNeutralResourcesFound(string baseName, Assembly mainAssembly, string resName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerNeutralResourcesFound(baseName, GetName(mainAssembly), resName);
            }
        }

        [Event(0x13, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerNeutralResourcesFound(string baseName, string mainAssemblyName, string resName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(0x13, baseName, mainAssemblyName, resName);
            }
        }

        [NonEvent]
        public void ResourceManagerNeutralResourcesNotFound(string baseName, Assembly mainAssembly, string resName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerNeutralResourcesNotFound(baseName, GetName(mainAssembly), resName);
            }
        }

        [Event(0x12, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerNeutralResourcesNotFound(string baseName, string mainAssemblyName, string resName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(0x12, baseName, mainAssemblyName, resName);
            }
        }

        [NonEvent]
        public void ResourceManagerNeutralResourcesSufficient(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerNeutralResourcesSufficient(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(12, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerNeutralResourcesSufficient(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(12, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerNotCreatingResourceSet(string baseName, Assembly mainAssembly, string cultureName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerNotCreatingResourceSet(baseName, GetName(mainAssembly), cultureName);
            }
        }

        [Event(15, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerNotCreatingResourceSet(string baseName, string mainAssemblyName, string cultureName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(15, baseName, mainAssemblyName, cultureName);
            }
        }

        [NonEvent]
        public void ResourceManagerReleasingResources(string baseName, Assembly mainAssembly)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerReleasingResources(baseName, GetName(mainAssembly));
            }
        }

        [Event(0x11, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerReleasingResources(string baseName, string mainAssemblyName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(0x11, baseName, mainAssemblyName);
            }
        }

        [NonEvent]
        public void ResourceManagerStreamFound(string baseName, Assembly mainAssembly, string cultureName, Assembly loadedAssembly, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerStreamFound(baseName, GetName(mainAssembly), cultureName, GetName(loadedAssembly), resourceFileName);
            }
        }

        [Event(5, Level=EventLevel.Informational, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerStreamFound(string baseName, string mainAssemblyName, string cultureName, string loadedAssemblyName, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(5, new object[] { baseName, mainAssemblyName, cultureName, loadedAssemblyName, resourceFileName });
            }
        }

        [NonEvent]
        public void ResourceManagerStreamNotFound(string baseName, Assembly mainAssembly, string cultureName, Assembly loadedAssembly, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                this.ResourceManagerStreamNotFound(baseName, GetName(mainAssembly), cultureName, GetName(loadedAssembly), resourceFileName);
            }
        }

        [Event(6, Level=EventLevel.Warning, Task=(EventTask) 1, Keywords=1)]
        public void ResourceManagerStreamNotFound(string baseName, string mainAssemblyName, string cultureName, string loadedAssemblyName, string resourceFileName)
        {
            if (base.IsEnabled())
            {
                base.WriteEvent(6, new object[] { baseName, mainAssemblyName, cultureName, loadedAssemblyName, resourceFileName });
            }
        }

        public static bool IsInitialized
        {
            get
            {
                return (Log != null);
            }
        }
    }
}

