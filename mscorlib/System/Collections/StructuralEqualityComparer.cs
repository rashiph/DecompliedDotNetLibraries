namespace System.Collections
{
    using System;

    [Serializable]
    internal class StructuralEqualityComparer : IEqualityComparer
    {
        public bool Equals(object x, object y)
        {
            if (x != null)
            {
                IStructuralEquatable equatable = x as IStructuralEquatable;
                if (equatable != null)
                {
                    return equatable.Equals(y, this);
                }
                return ((y != null) && x.Equals(y));
            }
            if (y != null)
            {
                return false;
            }
            return true;
        }

        public int GetHashCode(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            IStructuralEquatable equatable = obj as IStructuralEquatable;
            if (equatable != null)
            {
                return equatable.GetHashCode(this);
            }
            return obj.GetHashCode();
        }
    }
}

