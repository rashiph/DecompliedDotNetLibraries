namespace System.Runtime.InteropServices
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Policy;

    [Guid("17156360-2f1a-384a-bc52-fde93c215c5b"), TypeLibImportClass(typeof(Assembly)), CLSCompliant(false), ComVisible(true), InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface _Assembly
    {
        string ToString();
        bool Equals(object other);
        int GetHashCode();
        Type GetType();
        string CodeBase { get; }
        string EscapedCodeBase { get; }
        AssemblyName GetName();
        AssemblyName GetName(bool copiedName);
        string FullName { get; }
        MethodInfo EntryPoint { get; }
        Type GetType(string name);
        Type GetType(string name, bool throwOnError);
        Type[] GetExportedTypes();
        Type[] GetTypes();
        Stream GetManifestResourceStream(Type type, string name);
        Stream GetManifestResourceStream(string name);
        FileStream GetFile(string name);
        FileStream[] GetFiles();
        FileStream[] GetFiles(bool getResourceModules);
        string[] GetManifestResourceNames();
        ManifestResourceInfo GetManifestResourceInfo(string resourceName);
        string Location { get; }
        System.Security.Policy.Evidence Evidence { get; }
        object[] GetCustomAttributes(Type attributeType, bool inherit);
        object[] GetCustomAttributes(bool inherit);
        bool IsDefined(Type attributeType, bool inherit);
        [SecurityCritical]
        void GetObjectData(SerializationInfo info, StreamingContext context);
        Type GetType(string name, bool throwOnError, bool ignoreCase);
        Assembly GetSatelliteAssembly(CultureInfo culture);
        Assembly GetSatelliteAssembly(CultureInfo culture, Version version);
        Module LoadModule(string moduleName, byte[] rawModule);
        Module LoadModule(string moduleName, byte[] rawModule, byte[] rawSymbolStore);
        object CreateInstance(string typeName);
        object CreateInstance(string typeName, bool ignoreCase);
        object CreateInstance(string typeName, bool ignoreCase, BindingFlags bindingAttr, Binder binder, object[] args, CultureInfo culture, object[] activationAttributes);
        Module[] GetLoadedModules();
        Module[] GetLoadedModules(bool getResourceModules);
        Module[] GetModules();
        Module[] GetModules(bool getResourceModules);
        Module GetModule(string name);
        AssemblyName[] GetReferencedAssemblies();
        bool GlobalAssemblyCache { get; }
        event ModuleResolveEventHandler ModuleResolve;
    }
}

