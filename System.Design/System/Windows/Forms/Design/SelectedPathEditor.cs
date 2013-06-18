namespace System.Windows.Forms.Design
{
    using System;
    using System.Design;

    internal class SelectedPathEditor : FolderNameEditor
    {
        protected override void InitializeDialog(FolderNameEditor.FolderBrowser folderBrowser)
        {
            folderBrowser.Description = System.Design.SR.GetString("SelectedPathEditorLabel");
        }
    }
}

