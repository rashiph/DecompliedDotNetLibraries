namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;

    public abstract class CreateManifestResourceName : TaskExtension
    {
        private ITaskItem[] manifestResourceNames;
        private bool prependCultureAsDirectory = true;
        private ITaskItem[] resourceFiles;
        private ITaskItem[] resourceFilesWithManifestResourceNames;
        private string rootNamespace;

        protected CreateManifestResourceName()
        {
        }

        private Stream CreateFileStreamOverNewFileStream(string path, FileMode mode, FileAccess access)
        {
            return new FileStream(path, mode, access);
        }

        protected abstract string CreateManifestName(string fileName, string linkFileName, string rootNamespaceName, string dependentUponFileName, Stream binaryStream);
        public override bool Execute()
        {
            return this.Execute(new CreateFileStream(this.CreateFileStreamOverNewFileStream));
        }

        internal bool Execute(CreateFileStream createFileStream)
        {
            this.manifestResourceNames = new TaskItem[this.ResourceFiles.Length];
            this.resourceFilesWithManifestResourceNames = new TaskItem[this.ResourceFiles.Length];
            bool flag = true;
            int index = 0;
            if (this.RootNamespace != null)
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "CreateManifestResourceName.RootNamespace", new object[] { this.rootNamespace });
            }
            else
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "CreateManifestResourceName.NoRootNamespace", new object[0]);
            }
            foreach (ITaskItem item in this.ResourceFiles)
            {
                try
                {
                    string str3;
                    string itemSpec = item.ItemSpec;
                    string metadata = item.GetMetadata("DependentUpon");
                    bool flag2 = ((metadata != null) && (metadata.Length > 0)) && this.IsSourceFile(metadata);
                    if (flag2)
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "CreateManifestResourceName.DependsUpon", new object[] { itemSpec, metadata });
                    }
                    else
                    {
                        base.Log.LogMessageFromResources(MessageImportance.Low, "CreateManifestResourceName.DependsUponNothing", new object[] { itemSpec });
                    }
                    Stream binaryStream = null;
                    if (flag2)
                    {
                        string path = Path.Combine(Path.GetDirectoryName(itemSpec), metadata);
                        binaryStream = createFileStream(path, FileMode.Open, FileAccess.Read);
                    }
                    using (binaryStream)
                    {
                        str3 = this.CreateManifestName(itemSpec, item.GetMetadata("TargetPath"), this.RootNamespace, flag2 ? metadata : null, binaryStream);
                    }
                    this.manifestResourceNames[index] = new TaskItem(item);
                    this.manifestResourceNames[index].ItemSpec = str3;
                    this.resourceFilesWithManifestResourceNames[index] = new TaskItem(item);
                    this.resourceFilesWithManifestResourceNames[index].SetMetadata("ManifestResourceName", str3);
                    if (string.IsNullOrEmpty(this.resourceFilesWithManifestResourceNames[index].GetMetadata("LogicalName")) && string.Equals(this.resourceFilesWithManifestResourceNames[index].GetMetadata("Type"), "Non-Resx", StringComparison.OrdinalIgnoreCase))
                    {
                        this.resourceFilesWithManifestResourceNames[index].SetMetadata("LogicalName", str3);
                    }
                    base.Log.LogMessageFromResources(MessageImportance.Low, "CreateManifestResourceName.AssignedName", new object[] { itemSpec, str3 });
                }
                catch (Exception exception)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("CreateManifestResourceName.Error", new object[] { item.ItemSpec, exception.Message });
                    flag = false;
                }
                index++;
            }
            return flag;
        }

        protected abstract bool IsSourceFile(string fileName);
        private static bool IsValidEverettIdChar(char c)
        {
            UnicodeCategory unicodeCategory = char.GetUnicodeCategory(c);
            if ((!char.IsLetterOrDigit(c) && (unicodeCategory != UnicodeCategory.ConnectorPunctuation)) && ((unicodeCategory != UnicodeCategory.NonSpacingMark) && (unicodeCategory != UnicodeCategory.SpacingCombiningMark)))
            {
                return (unicodeCategory == UnicodeCategory.EnclosingMark);
            }
            return true;
        }

        private static bool IsValidEverettIdFirstChar(char c)
        {
            if (!char.IsLetter(c))
            {
                return (char.GetUnicodeCategory(c) == UnicodeCategory.ConnectorPunctuation);
            }
            return true;
        }

        internal static string MakeValidEverettFolderIdentifier(string name)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(name, "name");
            StringBuilder builder = new StringBuilder(name.Length + 1);
            string[] strArray = name.Split(new char[] { '.' });
            builder.Append(MakeValidEverettSubFolderIdentifier(strArray[0]));
            for (int i = 1; i < strArray.Length; i++)
            {
                builder.Append('.');
                builder.Append(MakeValidEverettSubFolderIdentifier(strArray[i]));
            }
            if (builder.ToString() == "_")
            {
                builder.Append('_');
            }
            return builder.ToString();
        }

        public static string MakeValidEverettIdentifier(string name)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(name, "name");
            StringBuilder builder = new StringBuilder(name.Length);
            string[] strArray = name.Split(new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar });
            builder.Append(MakeValidEverettFolderIdentifier(strArray[0]));
            for (int i = 1; i < strArray.Length; i++)
            {
                builder.Append('.');
                builder.Append(MakeValidEverettFolderIdentifier(strArray[i]));
            }
            return builder.ToString();
        }

        private static string MakeValidEverettSubFolderIdentifier(string subName)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(subName, "subName");
            if (subName.Length == 0)
            {
                return subName;
            }
            StringBuilder builder = new StringBuilder(subName.Length + 1);
            if (!IsValidEverettIdFirstChar(subName[0]))
            {
                if (!IsValidEverettIdChar(subName[0]))
                {
                    builder.Append('_');
                }
                else
                {
                    builder.Append('_');
                    builder.Append(subName[0]);
                }
            }
            else
            {
                builder.Append(subName[0]);
            }
            for (int i = 1; i < subName.Length; i++)
            {
                if (!IsValidEverettIdChar(subName[i]))
                {
                    builder.Append('_');
                }
                else
                {
                    builder.Append(subName[i]);
                }
            }
            return builder.ToString();
        }

        [Output]
        public ITaskItem[] ManifestResourceNames
        {
            get
            {
                return this.manifestResourceNames;
            }
        }

        public bool PrependCultureAsDirectory
        {
            get
            {
                return this.prependCultureAsDirectory;
            }
            set
            {
                this.prependCultureAsDirectory = value;
            }
        }

        [Required]
        public ITaskItem[] ResourceFiles
        {
            get
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(this.resourceFiles, "resourceFiles");
                return this.resourceFiles;
            }
            set
            {
                this.resourceFiles = value;
            }
        }

        [Output]
        public ITaskItem[] ResourceFilesWithManifestResourceNames
        {
            get
            {
                return this.resourceFilesWithManifestResourceNames;
            }
            set
            {
                this.resourceFilesWithManifestResourceNames = value;
            }
        }

        public string RootNamespace
        {
            get
            {
                return this.rootNamespace;
            }
            set
            {
                this.rootNamespace = value;
            }
        }
    }
}

