namespace System.DirectoryServices.ActiveDirectory
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class TopLevelNameCollection : ReadOnlyCollectionBase
    {
        internal TopLevelNameCollection()
        {
        }

        internal int Add(TopLevelName name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return base.InnerList.Add(name);
        }

        public bool Contains(TopLevelName name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return base.InnerList.Contains(name);
        }

        public void CopyTo(TopLevelName[] names, int index)
        {
            base.InnerList.CopyTo(names, index);
        }

        public int IndexOf(TopLevelName name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return base.InnerList.IndexOf(name);
        }

        public TopLevelName this[int index]
        {
            get
            {
                return (TopLevelName) base.InnerList[index];
            }
        }
    }
}

