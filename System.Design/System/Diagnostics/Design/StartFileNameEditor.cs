namespace System.Diagnostics.Design
{
    using System;
    using System.Design;
    using System.Security.Permissions;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    internal class StartFileNameEditor : FileNameEditor
    {
        protected override void InitializeDialog(OpenFileDialog openFile)
        {
            openFile.Filter = System.Design.SR.GetString("StartFileNameEditorAllFiles");
            openFile.Title = System.Design.SR.GetString("StartFileNameEditorTitle");
        }
    }
}

