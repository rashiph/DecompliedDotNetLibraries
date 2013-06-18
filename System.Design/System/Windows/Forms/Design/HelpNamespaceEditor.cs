namespace System.Windows.Forms.Design
{
    using System;
    using System.Design;
    using System.Windows.Forms;

    internal class HelpNamespaceEditor : FileNameEditor
    {
        protected override void InitializeDialog(OpenFileDialog openFileDialog)
        {
            openFileDialog.Filter = System.Design.SR.GetString("HelpProviderEditorFilter");
            openFileDialog.Title = System.Design.SR.GetString("HelpProviderEditorTitle");
        }
    }
}

