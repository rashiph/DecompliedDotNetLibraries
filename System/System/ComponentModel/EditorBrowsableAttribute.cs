namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Delegate | AttributeTargets.Interface | AttributeTargets.Event | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Enum | AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class EditorBrowsableAttribute : Attribute
    {
        private EditorBrowsableState browsableState;

        public EditorBrowsableAttribute() : this(EditorBrowsableState.Always)
        {
        }

        public EditorBrowsableAttribute(EditorBrowsableState state)
        {
            this.browsableState = state;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            EditorBrowsableAttribute attribute = obj as EditorBrowsableAttribute;
            return ((attribute != null) && (attribute.browsableState == this.browsableState));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public EditorBrowsableState State
        {
            get
            {
                return this.browsableState;
            }
        }
    }
}

