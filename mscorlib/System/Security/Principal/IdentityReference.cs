namespace System.Security.Principal
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(false)]
    public abstract class IdentityReference
    {
        internal IdentityReference()
        {
        }

        public abstract override bool Equals(object o);
        public abstract override int GetHashCode();
        public abstract bool IsValidTargetType(Type targetType);
        public static bool operator ==(IdentityReference left, IdentityReference right)
        {
            object obj2 = left;
            object obj3 = right;
            return (((obj2 == null) && (obj3 == null)) || (((obj2 != null) && (obj3 != null)) && left.Equals(right)));
        }

        public static bool operator !=(IdentityReference left, IdentityReference right)
        {
            return !(left == right);
        }

        public abstract override string ToString();
        public abstract IdentityReference Translate(Type targetType);

        public abstract string Value { get; }
    }
}

