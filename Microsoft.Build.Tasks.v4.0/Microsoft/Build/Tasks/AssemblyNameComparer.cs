namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal sealed class AssemblyNameComparer : IComparer, IHashCodeProvider, IEqualityComparer<AssemblyNameExtension>
    {
        internal static readonly IComparer comparer = new AssemblyNameComparer();
        internal static readonly IEqualityComparer<AssemblyNameExtension> genericComparer = (comparer as IEqualityComparer<AssemblyNameExtension>);

        private AssemblyNameComparer()
        {
        }

        public int Compare(object o1, object o2)
        {
            AssemblyNameExtension extension = (AssemblyNameExtension) o1;
            AssemblyNameExtension that = (AssemblyNameExtension) o2;
            return extension.CompareTo(that);
        }

        public bool Equals(AssemblyNameExtension x, AssemblyNameExtension y)
        {
            return x.Equals(y);
        }

        public bool Equals(object o1, object o2)
        {
            AssemblyNameExtension x = (AssemblyNameExtension) o1;
            AssemblyNameExtension y = (AssemblyNameExtension) o2;
            return this.Equals(x, y);
        }

        public int GetHashCode(AssemblyNameExtension obj)
        {
            return obj.GetHashCode();
        }

        public int GetHashCode(object o)
        {
            AssemblyNameExtension extension = (AssemblyNameExtension) o;
            return this.GetHashCode(extension);
        }
    }
}

