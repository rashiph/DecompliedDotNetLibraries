namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, ComVisible(true), Flags]
    public enum CspProviderFlags
    {
        CreateEphemeralKey = 0x80,
        NoFlags = 0,
        NoPrompt = 0x40,
        UseArchivableKey = 0x10,
        UseDefaultKeyContainer = 2,
        UseExistingKey = 8,
        UseMachineKeyStore = 1,
        UseNonExportableKey = 4,
        UseUserProtectedKey = 0x20
    }
}

