namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class PersistenceModeAttribute : System.Attribute
    {
        public static readonly PersistenceModeAttribute Attribute = new PersistenceModeAttribute(PersistenceMode.Attribute);
        public static readonly PersistenceModeAttribute Default = Attribute;
        public static readonly PersistenceModeAttribute EncodedInnerDefaultProperty = new PersistenceModeAttribute(PersistenceMode.EncodedInnerDefaultProperty);
        public static readonly PersistenceModeAttribute InnerDefaultProperty = new PersistenceModeAttribute(PersistenceMode.InnerDefaultProperty);
        public static readonly PersistenceModeAttribute InnerProperty = new PersistenceModeAttribute(PersistenceMode.InnerProperty);
        private PersistenceMode mode;

        public PersistenceModeAttribute(PersistenceMode mode)
        {
            if ((mode < PersistenceMode.Attribute) || (mode > PersistenceMode.EncodedInnerDefaultProperty))
            {
                throw new ArgumentOutOfRangeException("mode");
            }
            this.mode = mode;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is PersistenceModeAttribute)) && (((PersistenceModeAttribute) obj).Mode == this.mode)));
        }

        public override int GetHashCode()
        {
            return this.Mode.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public PersistenceMode Mode
        {
            get
            {
                return this.mode;
            }
        }
    }
}

