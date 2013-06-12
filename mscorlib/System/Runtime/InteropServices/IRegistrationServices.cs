namespace System.Runtime.InteropServices
{
    using System;
    using System.Reflection;
    using System.Security;

    [Guid("CCBD682C-73A5-4568-B8B0-C7007E11ABA2"), ComVisible(true)]
    public interface IRegistrationServices
    {
        [SecurityCritical]
        bool RegisterAssembly(Assembly assembly, AssemblyRegistrationFlags flags);
        [SecurityCritical]
        bool UnregisterAssembly(Assembly assembly);
        [SecurityCritical]
        Type[] GetRegistrableTypesInAssembly(Assembly assembly);
        [SecurityCritical]
        string GetProgIdForType(Type type);
        [SecurityCritical]
        void RegisterTypeForComClients(Type type, ref Guid g);
        Guid GetManagedCategoryGuid();
        [SecurityCritical]
        bool TypeRequiresRegistration(Type type);
        bool TypeRepresentsComType(Type type);
    }
}

