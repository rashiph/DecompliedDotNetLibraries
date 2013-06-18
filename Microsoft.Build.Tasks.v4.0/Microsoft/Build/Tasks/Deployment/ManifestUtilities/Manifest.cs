namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public abstract class Manifest
    {
        private Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity assemblyIdentity;
        private AssemblyReferenceCollection assemblyReferenceList;
        private AssemblyReference[] assemblyReferences;
        private string description;
        private FileReferenceCollection fileReferenceList;
        private FileReference[] fileReferences;
        private Stream inputStream;
        private readonly OutputMessageCollection outputMessages = new OutputMessageCollection();
        private bool readOnly;
        private string sourcePath;
        private bool treatUnfoundNativeAssembliesAsPrerequisites;

        protected internal Manifest()
        {
        }

        private void CollectionToArray()
        {
            if (this.assemblyReferenceList != null)
            {
                this.assemblyReferences = this.assemblyReferenceList.ToArray();
                this.assemblyReferenceList = null;
            }
            if (this.fileReferenceList != null)
            {
                this.fileReferences = this.fileReferenceList.ToArray();
                this.fileReferenceList = null;
            }
        }

        private bool IsMismatchedPlatform(AssemblyReference assembly)
        {
            if (assembly.IsVirtual)
            {
                return false;
            }
            if ((this.AssemblyIdentity == null) || (assembly.AssemblyIdentity == null))
            {
                return false;
            }
            if (this.AssemblyIdentity.IsNeutralPlatform)
            {
                return ((assembly.ReferenceType == AssemblyReferenceType.NativeAssembly) || !assembly.AssemblyIdentity.IsNeutralPlatform);
            }
            if ((assembly != this.EntryPoint) && assembly.AssemblyIdentity.IsNeutralPlatform)
            {
                return false;
            }
            return !string.Equals(this.AssemblyIdentity.ProcessorArchitecture, assembly.AssemblyIdentity.ProcessorArchitecture, StringComparison.OrdinalIgnoreCase);
        }

        internal virtual void OnAfterLoad()
        {
        }

        internal virtual void OnBeforeSave()
        {
            this.CollectionToArray();
            this.SortFiles();
        }

        private bool ResolveAssembly(AssemblyReference a, string[] searchPaths)
        {
            if (a == null)
            {
                return false;
            }
            a.ResolvedPath = this.ResolvePath(a.SourcePath, searchPaths);
            if (!string.IsNullOrEmpty(a.ResolvedPath))
            {
                return true;
            }
            if (a.AssemblyIdentity != null)
            {
                a.ResolvedPath = a.AssemblyIdentity.Resolve(searchPaths);
                if (!string.IsNullOrEmpty(a.ResolvedPath))
                {
                    return true;
                }
            }
            a.ResolvedPath = this.ResolvePath(a.TargetPath, searchPaths);
            return !string.IsNullOrEmpty(a.ResolvedPath);
        }

        private bool ResolveFile(BaseReference f, string[] searchPaths)
        {
            if (f == null)
            {
                return false;
            }
            f.ResolvedPath = this.ResolvePath(f.SourcePath, searchPaths);
            if (!string.IsNullOrEmpty(f.ResolvedPath))
            {
                return true;
            }
            f.ResolvedPath = this.ResolvePath(f.TargetPath, searchPaths);
            return !string.IsNullOrEmpty(f.ResolvedPath);
        }

        public void ResolveFiles()
        {
            string path = string.Empty;
            if (!string.IsNullOrEmpty(this.sourcePath))
            {
                path = Path.GetDirectoryName(this.sourcePath);
            }
            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(Environment.CurrentDirectory, path);
            }
            string[] searchPaths = new string[] { path };
            this.ResolveFiles(searchPaths);
        }

        public void ResolveFiles(string[] searchPaths)
        {
            if (searchPaths == null)
            {
                throw new ArgumentNullException("searchPaths");
            }
            this.CollectionToArray();
            this.ResolveFiles_1(searchPaths);
            this.ResolveFiles_2(searchPaths);
        }

        private void ResolveFiles_1(string[] searchPaths)
        {
            if (this.assemblyReferences != null)
            {
                foreach (AssemblyReference reference in this.assemblyReferences)
                {
                    if ((!reference.IsPrerequisite || (reference.AssemblyIdentity == null)) && !this.ResolveAssembly(reference, searchPaths))
                    {
                        if (this.treatUnfoundNativeAssembliesAsPrerequisites && (reference.ReferenceType == AssemblyReferenceType.NativeAssembly))
                        {
                            reference.IsPrerequisite = true;
                        }
                        else if (this.readOnly)
                        {
                            this.OutputMessages.AddErrorMessage("GenerateManifest.ResolveFailedInReadOnlyMode", new string[] { reference.ToString(), this.ToString() });
                        }
                        else
                        {
                            this.OutputMessages.AddErrorMessage("GenerateManifest.ResolveFailedInReadWriteMode", new string[] { reference.ToString() });
                        }
                    }
                }
            }
        }

        private void ResolveFiles_2(string[] searchPaths)
        {
            if (this.fileReferences != null)
            {
                foreach (FileReference reference in this.fileReferences)
                {
                    if (!this.ResolveFile(reference, searchPaths))
                    {
                        if (this.readOnly)
                        {
                            this.OutputMessages.AddErrorMessage("GenerateManifest.ResolveFailedInReadOnlyMode", new string[] { reference.ToString(), this.ToString() });
                        }
                        else
                        {
                            this.OutputMessages.AddErrorMessage("GenerateManifest.ResolveFailedInReadWriteMode", new string[] { reference.ToString() });
                        }
                    }
                }
            }
        }

        private string ResolvePath(string path, string[] searchPaths)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (Path.IsPathRooted(path))
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    return null;
                }
                if (searchPaths != null)
                {
                    foreach (string str in searchPaths)
                    {
                        if (!string.IsNullOrEmpty(str))
                        {
                            string fullPath = Path.GetFullPath(Path.Combine(str, path));
                            if (File.Exists(fullPath))
                            {
                                return fullPath;
                            }
                        }
                    }
                }
            }
            return null;
        }

        private void SortFiles()
        {
            this.CollectionToArray();
            ReferenceComparer comparer = new ReferenceComparer();
            if (this.assemblyReferences != null)
            {
                Array.Sort(this.assemblyReferences, comparer);
            }
            if (this.fileReferences != null)
            {
                Array.Sort(this.fileReferences, comparer);
            }
        }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(this.sourcePath))
            {
                return this.sourcePath;
            }
            return this.AssemblyIdentity.ToString();
        }

        private void UpdateAssemblyReference(AssemblyReference a)
        {
            if (a.IsVirtual)
            {
                return;
            }
            if (a.AssemblyIdentity == null)
            {
                switch (a.ReferenceType)
                {
                    case AssemblyReferenceType.ClickOnceManifest:
                        a.AssemblyIdentity = Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity.FromManifest(a.ResolvedPath);
                        goto Label_0078;

                    case AssemblyReferenceType.ManagedAssembly:
                        a.AssemblyIdentity = Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity.FromManagedAssembly(a.ResolvedPath);
                        goto Label_0078;

                    case AssemblyReferenceType.NativeAssembly:
                        a.AssemblyIdentity = Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity.FromNativeAssembly(a.ResolvedPath);
                        goto Label_0078;
                }
                a.AssemblyIdentity = Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity.FromFile(a.ResolvedPath);
            }
        Label_0078:
            if (!a.IsPrerequisite)
            {
                UpdateFileReference(a);
            }
            if (a.ReferenceType == AssemblyReferenceType.Unspecified)
            {
                if (this is DeployManifest)
                {
                    a.ReferenceType = AssemblyReferenceType.ClickOnceManifest;
                }
                else if (!string.IsNullOrEmpty(a.ResolvedPath))
                {
                    if (PathUtil.IsNativeAssembly(a.ResolvedPath))
                    {
                        a.ReferenceType = AssemblyReferenceType.NativeAssembly;
                    }
                    else
                    {
                        a.ReferenceType = AssemblyReferenceType.ManagedAssembly;
                    }
                }
                else if ((a.AssemblyIdentity != null) && string.Equals(a.AssemblyIdentity.Type, "win32", StringComparison.OrdinalIgnoreCase))
                {
                    a.ReferenceType = AssemblyReferenceType.NativeAssembly;
                }
            }
        }

        internal static void UpdateEntryPoint(string inputPath, string outputPath, string updatedApplicationPath, string applicationManifestPath)
        {
            string str2;
            long num;
            XmlDocument document = new XmlDocument();
            document.Load(inputPath);
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(document.NameTable);
            Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity identity = Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity.FromManifest(applicationManifestPath);
            XmlNode node = null;
            foreach (string str in XPaths.codebasePaths)
            {
                node = document.SelectSingleNode(str, namespaceManager);
                if (node != null)
                {
                    break;
                }
            }
            if (node == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "XPath not found: {0}", new object[] { XPaths.codebasePaths[0] }));
            }
            node.Value = updatedApplicationPath;
            XmlNode node2 = ((XmlAttribute) node).OwnerElement.SelectSingleNode("asmv2:assemblyIdentity/@publicKeyToken", namespaceManager);
            if (node2 == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "XPath not found: {0}", new object[] { "asmv2:assemblyIdentity/@publicKeyToken" }));
            }
            node2.Value = identity.PublicKeyToken;
            Util.GetFileInfo(applicationManifestPath, out str2, out num);
            XmlNode node3 = ((XmlAttribute) node).OwnerElement.SelectSingleNode("asmv2:hash/dsig:DigestValue", namespaceManager);
            if (node3 != null)
            {
                ((XmlElement) node3).InnerText = str2;
            }
            XmlAttribute attribute = ((XmlAttribute) node).OwnerElement.Attributes[XmlUtil.TrimPrefix("asmv2:size")];
            if (attribute == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "XPath not found: {0}", new object[] { "asmv2:size" }));
            }
            attribute.Value = num.ToString(CultureInfo.InvariantCulture);
            document.Save(outputPath);
        }

        public void UpdateFileInfo()
        {
            if (this.assemblyReferences != null)
            {
                foreach (AssemblyReference reference in this.assemblyReferences)
                {
                    if (!string.IsNullOrEmpty(reference.ResolvedPath))
                    {
                        try
                        {
                            this.UpdateAssemblyReference(reference);
                            if (reference.AssemblyIdentity == null)
                            {
                                BadImageFormatException exception = new BadImageFormatException(null, reference.ResolvedPath);
                                this.OutputMessages.AddErrorMessage("GenerateManifest.General", new string[] { exception.Message });
                            }
                        }
                        catch (Exception exception2)
                        {
                            this.OutputMessages.AddErrorMessage("GenerateManifest.General", new string[] { exception2.Message });
                        }
                    }
                }
            }
            if (this.fileReferences != null)
            {
                foreach (FileReference reference2 in this.fileReferences)
                {
                    if (!string.IsNullOrEmpty(reference2.ResolvedPath))
                    {
                        try
                        {
                            UpdateFileReference(reference2);
                        }
                        catch (Exception exception3)
                        {
                            this.OutputMessages.AddErrorMessage("GenerateManifest.General", new string[] { exception3.Message });
                        }
                    }
                }
            }
        }

        private static void UpdateFileReference(BaseReference f)
        {
            string str;
            long num;
            if (string.IsNullOrEmpty(f.ResolvedPath))
            {
                throw new FileNotFoundException(null, f.SourcePath);
            }
            Util.GetFileInfo(f.ResolvedPath, out str, out num);
            f.Hash = str;
            f.Size = num;
            if (string.IsNullOrEmpty(f.TargetPath))
            {
                if (!string.IsNullOrEmpty(f.SourcePath))
                {
                    f.TargetPath = BaseReference.GetDefaultTargetPath(f.SourcePath);
                }
                else
                {
                    f.TargetPath = BaseReference.GetDefaultTargetPath(Path.GetFileName(f.ResolvedPath));
                }
            }
        }

        public virtual void Validate()
        {
            this.ValidateReferences();
        }

        protected void ValidatePlatform()
        {
            foreach (AssemblyReference reference in this.AssemblyReferences)
            {
                if (this.IsMismatchedPlatform(reference))
                {
                    this.OutputMessages.AddWarningMessage("GenerateManifest.PlatformMismatch", new string[] { reference.ToString() });
                }
            }
        }

        private void ValidateReferences()
        {
            if (this.AssemblyReferences.Count > 1)
            {
                Dictionary<string, NGen<bool>> dictionary = new Dictionary<string, NGen<bool>>();
                foreach (AssemblyReference reference in this.AssemblyReferences)
                {
                    if (reference.AssemblyIdentity != null)
                    {
                        string fullName = reference.AssemblyIdentity.GetFullName(Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity.FullNameFlags.All);
                        string key = fullName.ToLowerInvariant();
                        if (!dictionary.ContainsKey(key))
                        {
                            dictionary.Add(key, 0);
                        }
                        else if (dictionary[key] == 0)
                        {
                            this.OutputMessages.AddWarningMessage("GenerateManifest.DuplicateAssemblyIdentity", new string[] { fullName });
                            dictionary[key] = 1;
                        }
                    }
                    if ((!reference.IsPrerequisite && (reference.AssemblyIdentity != null)) && !string.Equals(reference.AssemblyIdentity.Name, Path.GetFileNameWithoutExtension(reference.TargetPath), StringComparison.OrdinalIgnoreCase))
                    {
                        this.OutputMessages.AddWarningMessage("GenerateManifest.IdentityFileNameMismatch", new string[] { reference.ToString(), reference.AssemblyIdentity.Name, reference.AssemblyIdentity.Name + Path.GetExtension(reference.TargetPath) });
                    }
                }
            }
        }

        [XmlIgnore]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity AssemblyIdentity
        {
            get
            {
                if (this.assemblyIdentity == null)
                {
                    this.assemblyIdentity = new Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity();
                }
                return this.assemblyIdentity;
            }
            set
            {
                this.assemblyIdentity = value;
            }
        }

        [XmlIgnore]
        public AssemblyReferenceCollection AssemblyReferences
        {
            get
            {
                if (this.assemblyReferenceList == null)
                {
                    this.assemblyReferenceList = new AssemblyReferenceCollection(this.assemblyReferences);
                }
                return this.assemblyReferenceList;
            }
        }

        [XmlIgnore]
        public string Description
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        [XmlIgnore]
        public virtual AssemblyReference EntryPoint
        {
            get
            {
                return null;
            }
            set
            {
            }
        }

        [XmlIgnore]
        public FileReferenceCollection FileReferences
        {
            get
            {
                if (this.fileReferenceList == null)
                {
                    this.fileReferenceList = new FileReferenceCollection(this.fileReferences);
                }
                return this.fileReferenceList;
            }
        }

        [XmlIgnore]
        public Stream InputStream
        {
            get
            {
                return this.inputStream;
            }
            set
            {
                this.inputStream = value;
            }
        }

        [XmlIgnore]
        public OutputMessageCollection OutputMessages
        {
            get
            {
                return this.outputMessages;
            }
        }

        [XmlIgnore]
        public bool ReadOnly
        {
            get
            {
                return this.readOnly;
            }
            set
            {
                this.readOnly = value;
            }
        }

        [XmlIgnore]
        public string SourcePath
        {
            get
            {
                return this.sourcePath;
            }
            set
            {
                this.sourcePath = value;
            }
        }

        internal bool TreatUnfoundNativeAssembliesAsPrerequisites
        {
            get
            {
                return this.treatUnfoundNativeAssembliesAsPrerequisites;
            }
            set
            {
                this.treatUnfoundNativeAssembliesAsPrerequisites = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlElement("AssemblyIdentity")]
        public Microsoft.Build.Tasks.Deployment.ManifestUtilities.AssemblyIdentity XmlAssemblyIdentity
        {
            get
            {
                return this.assemblyIdentity;
            }
            set
            {
                this.assemblyIdentity = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlArray("AssemblyReferences"), Browsable(false)]
        public AssemblyReference[] XmlAssemblyReferences
        {
            get
            {
                return this.assemblyReferences;
            }
            set
            {
                this.assemblyReferences = value;
            }
        }

        [XmlAttribute("Description"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public string XmlDescription
        {
            get
            {
                return this.description;
            }
            set
            {
                this.description = value;
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), XmlArray("FileReferences"), Browsable(false)]
        public FileReference[] XmlFileReferences
        {
            get
            {
                return this.fileReferences;
            }
            set
            {
                this.fileReferences = value;
            }
        }

        [XmlAttribute("Schema"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlSchema
        {
            get
            {
                return Util.Schema;
            }
            set
            {
            }
        }

        private class ReferenceComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if ((x != null) && (y != null))
                {
                    if (!(x is BaseReference) || !(y is BaseReference))
                    {
                        return 0;
                    }
                    BaseReference reference = x as BaseReference;
                    BaseReference reference2 = y as BaseReference;
                    if ((reference.SortName != null) && (reference2.SortName != null))
                    {
                        return reference.SortName.CompareTo(reference2.SortName);
                    }
                }
                return 0;
            }
        }
    }
}

