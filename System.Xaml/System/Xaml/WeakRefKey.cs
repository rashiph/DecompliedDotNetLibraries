namespace System.Xaml
{
    using System;

    internal class WeakRefKey : WeakReference
    {
        private int _hashCode;

        public WeakRefKey(object target) : base(target)
        {
            this._hashCode = target.GetHashCode();
        }

        public override bool Equals(object o)
        {
            WeakRefKey key = o as WeakRefKey;
            if (key != null)
            {
                object target = this.Target;
                object obj3 = key.Target;
                if ((target != null) && (obj3 != null))
                {
                    return (target == obj3);
                }
            }
            return base.Equals(o);
        }

        public override int GetHashCode()
        {
            return this._hashCode;
        }

        public static bool operator ==(WeakRefKey left, WeakRefKey right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public static bool operator !=(WeakRefKey left, WeakRefKey right)
        {
            return !(left == right);
        }
    }
}

