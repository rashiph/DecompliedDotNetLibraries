namespace System.Workflow.ComponentModel.Compiler
{
    using Microsoft.Build.Tasks;
    using System;
    using System.IO;

    internal sealed class CreateWorkflowManifestResourceNameForCSharp : CreateCSharpManifestResourceName
    {
        private bool lastAskedFileWasXoml;

        protected override string CreateManifestName(string fileName, string linkFileName, string rootNamespace, string dependentUponFileName, Stream binaryStream)
        {
            string str = string.Empty;
            if (!this.lastAskedFileWasXoml)
            {
                str = base.CreateManifestName(fileName, linkFileName, rootNamespace, dependentUponFileName, binaryStream);
            }
            else
            {
                str = TasksHelper.GetXomlManifestName(fileName, linkFileName, rootNamespace, binaryStream);
            }
            string extension = Path.GetExtension(fileName);
            if ((string.Compare(extension, ".rules", StringComparison.OrdinalIgnoreCase) == 0) || (string.Compare(extension, ".layout", StringComparison.OrdinalIgnoreCase) == 0))
            {
                str = str + extension;
            }
            this.lastAskedFileWasXoml = false;
            return str;
        }

        protected override bool IsSourceFile(string fileName)
        {
            if (string.Compare(Path.GetExtension(fileName), ".xoml", StringComparison.OrdinalIgnoreCase) == 0)
            {
                this.lastAskedFileWasXoml = true;
                return true;
            }
            return base.IsSourceFile(fileName);
        }
    }
}

