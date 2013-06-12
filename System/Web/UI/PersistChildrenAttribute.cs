namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PersistChildrenAttribute : Attribute
    {
        private bool _persist;
        private bool _usesCustomPersistence;
        public static readonly PersistChildrenAttribute Default = Yes;
        public static readonly PersistChildrenAttribute No = new PersistChildrenAttribute(false);
        public static readonly PersistChildrenAttribute Yes = new PersistChildrenAttribute(true);

        public PersistChildrenAttribute(bool persist)
        {
            this._persist = persist;
        }

        public PersistChildrenAttribute(bool persist, bool usesCustomPersistence) : this(persist)
        {
            this._usesCustomPersistence = usesCustomPersistence;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is PersistChildrenAttribute)) && (((PersistChildrenAttribute) obj).Persist == this._persist)));
        }

        public override int GetHashCode()
        {
            return this.Persist.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool Persist
        {
            get
            {
                return this._persist;
            }
        }

        public bool UsesCustomPersistence
        {
            get
            {
                return (!this._persist && this._usesCustomPersistence);
            }
        }
    }
}

