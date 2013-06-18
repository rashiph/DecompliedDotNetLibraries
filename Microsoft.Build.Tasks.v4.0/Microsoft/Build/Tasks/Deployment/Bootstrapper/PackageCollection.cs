namespace Microsoft.Build.Tasks.Deployment.Bootstrapper
{
    using System;
    using System.Collections;

    internal class PackageCollection : IEnumerable
    {
        private Hashtable cultures = new Hashtable();
        private ArrayList list = new ArrayList();

        internal void Add(Microsoft.Build.Tasks.Deployment.Bootstrapper.Package package)
        {
            if (!this.cultures.Contains(package.Culture.ToLowerInvariant()))
            {
                this.list.Add(package);
                this.cultures.Add(package.Culture.ToLowerInvariant(), package);
            }
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.Package Item(int index)
        {
            return (Microsoft.Build.Tasks.Deployment.Bootstrapper.Package) this.list[index];
        }

        public Microsoft.Build.Tasks.Deployment.Bootstrapper.Package Package(string culture)
        {
            if (this.cultures.Contains(culture.ToLowerInvariant()))
            {
                return (Microsoft.Build.Tasks.Deployment.Bootstrapper.Package) this.cultures[culture.ToLowerInvariant()];
            }
            return null;
        }

        public int Count
        {
            get
            {
                return this.list.Count;
            }
        }
    }
}

