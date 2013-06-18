namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;

    public class InputLanguageCollection : ReadOnlyCollectionBase
    {
        internal InputLanguageCollection(InputLanguage[] value)
        {
            base.InnerList.AddRange(value);
        }

        public bool Contains(InputLanguage value)
        {
            return base.InnerList.Contains(value);
        }

        public void CopyTo(InputLanguage[] array, int index)
        {
            base.InnerList.CopyTo(array, index);
        }

        public int IndexOf(InputLanguage value)
        {
            return base.InnerList.IndexOf(value);
        }

        public InputLanguage this[int index]
        {
            get
            {
                return (InputLanguage) base.InnerList[index];
            }
        }
    }
}

