namespace System.Data.Design
{
    using System;
    using System.Collections;

    internal sealed class DesignUtil
    {
        private DesignUtil()
        {
        }

        internal static IDictionary CloneDictionary(IDictionary source)
        {
            if (source == null)
            {
                return null;
            }
            if (source is ICloneable)
            {
                return (IDictionary) ((ICloneable) source).Clone();
            }
            IDictionary dictionary = (IDictionary) Activator.CreateInstance(source.GetType());
            IDictionaryEnumerator enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ICloneable key = enumerator.Key as ICloneable;
                ICloneable cloneable2 = enumerator.Value as ICloneable;
                if ((key != null) && (cloneable2 != null))
                {
                    dictionary.Add(key.Clone(), cloneable2.Clone());
                }
            }
            return dictionary;
        }
    }
}

