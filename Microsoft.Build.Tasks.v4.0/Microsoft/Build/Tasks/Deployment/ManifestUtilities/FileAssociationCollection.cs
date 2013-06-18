namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class FileAssociationCollection : IEnumerable
    {
        private List<FileAssociation> list = new List<FileAssociation>();

        internal FileAssociationCollection(FileAssociation[] fileAssociations)
        {
            if (fileAssociations != null)
            {
                this.list.AddRange(fileAssociations);
            }
        }

        public void Add(FileAssociation fileAssociation)
        {
            this.list.Add(fileAssociation);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        internal FileAssociation[] ToArray()
        {
            return this.list.ToArray();
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }

        public FileAssociation this[int index]
        {
            get
            {
                return this.list[index];
            }
        }
    }
}

