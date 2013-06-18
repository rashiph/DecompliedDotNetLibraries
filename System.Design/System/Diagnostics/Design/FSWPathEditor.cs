namespace System.Diagnostics.Design
{
    using System;
    using System.Design;
    using System.Security.Permissions;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class FSWPathEditor : FolderNameEditor
    {
        protected override void InitializeDialog(FolderNameEditor.FolderBrowser folderBrowser)
        {
            folderBrowser.Description = System.Design.SR.GetString("FSWPathEditorLabel");
        }
    }
}

