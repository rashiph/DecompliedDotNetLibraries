namespace System.Windows.Forms.ComponentModel.Com2Interop
{
    using Microsoft.Win32;
    using System;
    using System.ComponentModel.Design;

    public interface IComPropertyBrowser
    {
        event ComponentRenameEventHandler ComComponentNameChanged;

        void DropDownDone();
        bool EnsurePendingChangesCommitted();
        void HandleF4();
        void LoadState(RegistryKey key);
        void SaveState(RegistryKey key);

        bool InPropertySet { get; }
    }
}

