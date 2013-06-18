namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IDReferencePropertyAttribute : Attribute
    {
        private Type _referencedControlType;

        public IDReferencePropertyAttribute() : this(typeof(Control))
        {
        }

        public IDReferencePropertyAttribute(Type referencedControlType)
        {
            this._referencedControlType = referencedControlType;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            IDReferencePropertyAttribute attribute = obj as IDReferencePropertyAttribute;
            return ((attribute != null) && (this.ReferencedControlType == attribute.ReferencedControlType));
        }

        public override int GetHashCode()
        {
            if (this.ReferencedControlType == null)
            {
                return 0;
            }
            return this.ReferencedControlType.GetHashCode();
        }

        public Type ReferencedControlType
        {
            get
            {
                return this._referencedControlType;
            }
        }
    }
}

