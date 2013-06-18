namespace System.Data.Design
{
    using System;
    using System.Collections;

    internal class NamedObjectUtil
    {
        private NamedObjectUtil()
        {
        }

        public static INamedObject Find(INamedObjectCollection coll, string name)
        {
            return Find(coll, name, false);
        }

        private static INamedObject Find(ICollection coll, string name, bool ignoreCase)
        {
            IEnumerator enumerator = coll.GetEnumerator();
            while (enumerator.MoveNext())
            {
                INamedObject current = enumerator.Current as INamedObject;
                if (current == null)
                {
                    throw new InternalException("Named object collection holds something that is not a named object", 2);
                }
                if (StringUtil.EqualValue(current.Name, name, ignoreCase))
                {
                    return current;
                }
            }
            return null;
        }
    }
}

