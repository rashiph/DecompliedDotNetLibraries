namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class FileReferenceCollection : IEnumerable
    {
        private ArrayList list = new ArrayList();

        internal FileReferenceCollection(FileReference[] array)
        {
            if (array != null)
            {
                this.list.AddRange(array);
            }
        }

        public FileReference Add(FileReference file)
        {
            this.list.Add(file);
            return file;
        }

        public FileReference Add(string path)
        {
            return this.Add(new FileReference(path));
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public FileReference FindTargetPath(string targetPath)
        {
            if (!string.IsNullOrEmpty(targetPath))
            {
                foreach (FileReference reference in this.list)
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

        public void Remove(FileReference file)
        {
            this.list.Remove(file);
        }

        internal FileReference[] ToArray()
        {
            return (FileReference[]) this.list.ToArray(typeof(FileReference));
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public FileReference this[int index]
        {
            get
            {
                return (FileReference) this.list[index];
            }
        }
    }
}

