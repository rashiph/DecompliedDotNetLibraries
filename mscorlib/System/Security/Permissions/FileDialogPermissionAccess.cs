namespace System.Security.Permissions
{
    using System;
    using System.Runtime.InteropServices;

    [Serializable, Flags, ComVisible(true)]
    public enum FileDialogPermissionAccess
    {
        None,
        Open,
        Save,
        OpenSave
    }
}

