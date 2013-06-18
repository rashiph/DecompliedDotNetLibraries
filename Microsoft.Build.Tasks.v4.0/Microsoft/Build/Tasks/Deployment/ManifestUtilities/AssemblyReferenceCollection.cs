namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class AssemblyReferenceCollection : IEnumerable
    {
        private ArrayList list = new ArrayList();

        internal AssemblyReferenceCollection(AssemblyReference[] array)
        {
            if (array != null)
            {
                this.list.AddRange(array);
            }
        }

        public AssemblyReference Add(AssemblyReference assembly)
        {
            this.list.Add(assembly);
            return assembly;
        }

        public AssemblyReference Add(string path)
        {
            return this.Add(new AssemblyReference(path));
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public AssemblyReference Find(AssemblyIdentity identity)
        {
            if (identity != null)
            {
                foreach (AssemblyReference reference in this.list)
                {
                    AssemblyIdentity assemblyIdentity = reference.AssemblyIdentity;
                    if ((((assemblyIdentity == null) && (identity.Name != null)) && ((reference.SourcePath != null) && (reference.ReferenceType == AssemblyReferenceType.ManagedAssembly))) && string.Equals(identity.Name, Path.GetFileNameWithoutExtension(reference.SourcePath), StringComparison.OrdinalIgnoreCase))
                    {
                        assemblyIdentity = AssemblyIdentity.FromManagedAssembly(reference.SourcePath);
                    }
                    if (AssemblyIdentity.IsEqual(assemblyIdentity, identity))
                    {
                        return reference;
                    }
                }
            }
            return null;
        }

        public AssemblyReference Find(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                foreach (AssemblyReference reference in this.list)
                {
                    if ((reference.AssemblyIdentity != null) && (string.Compare(name, reference.AssemblyIdentity.Name, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        return reference;
                    }
                }
            }
            return null;
        }

        public AssemblyReference FindTargetPath(string targetPath)
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                foreach (AssemblyReference reference in this.list)
                {
                    if (string.Compare(targetPath, reference.TargetPath, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return reference;
                    }
                }
            }
            return null;
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public void Remove(AssemblyReference assemblyReference)
        {
            this.list.Remove(assemblyReference);
        }

        internal AssemblyReference[] ToArray()
        {
            return (AssemblyReference[]) this.list.ToArray(typeof(AssemblyReference));
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public AssemblyReference this[int index]
        {
            get
            {
                return (AssemblyReference) this.list[index];
            }
        }
    }
}

