namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class NonVisualControlAttribute : Attribute
    {
        private bool _nonVisual;
        public static readonly NonVisualControlAttribute Default = Visual;
        public static readonly NonVisualControlAttribute NonVisual = new NonVisualControlAttribute(true);
        public static readonly NonVisualControlAttribute Visual = new NonVisualControlAttribute(false);

        public NonVisualControlAttribute() : this(true)
        {
        }

        public NonVisualControlAttribute(bool nonVisual)
        {
            this._nonVisual = nonVisual;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            NonVisualControlAttribute attribute = obj as NonVisualControlAttribute;
            return ((attribute != null) && (attribute.IsNonVisual == this.IsNonVisual));
        }

        public override int GetHashCode()
        {
            return this._nonVisual.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public bool IsNonVisual
        {
            get
            {
                return this._nonVisual;
            }
        }
    }
}

