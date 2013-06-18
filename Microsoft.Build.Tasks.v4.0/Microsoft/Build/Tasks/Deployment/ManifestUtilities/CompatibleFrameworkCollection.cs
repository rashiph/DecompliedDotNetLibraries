namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public sealed class CompatibleFrameworkCollection : IEnumerable
    {
        private List<CompatibleFramework> list = new List<CompatibleFramework>();

        internal CompatibleFrameworkCollection(CompatibleFramework[] compatibleFrameworks)
        {
            if (compatibleFrameworks != null)
            {
                this.list.AddRange(compatibleFrameworks);
            }
        }

        public void Add(CompatibleFramework compatibleFramework)
        {
            this.list.Add(compatibleFramework);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public IEnumerator GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        internal CompatibleFramework[] ToArray()
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

        public CompatibleFramework this[int index]
        {
            get
            {
                return this.list[index];
            }
        }
    }
}

