namespace System.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public interface IXamlNameResolver
    {
        event EventHandler OnNameScopeInitializationComplete;

        IEnumerable<KeyValuePair<string, object>> GetAllNamesAndValuesInScope();
        object GetFixupToken(IEnumerable<string> names);
        object GetFixupToken(IEnumerable<string> names, bool canAssignDirectly);
        object Resolve(string name);
        object Resolve(string name, out bool isFullyInitialized);

        bool IsFixupTokenAvailable { get; }
    }
}

