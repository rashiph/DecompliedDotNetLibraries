namespace System.Web.Util
{
    using System;
    using System.Collections;

    internal class AssemblySet : ObjectSet
    {
        internal AssemblySet()
        {
        }

        internal static AssemblySet Create(ICollection c)
        {
            AssemblySet set = new AssemblySet();
            set.AddCollection(c);
            return set;
        }
    }
}

