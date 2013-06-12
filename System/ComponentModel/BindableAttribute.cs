namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class BindableAttribute : Attribute
    {
        private bool bindable;
        public static readonly BindableAttribute Default = No;
        private BindingDirection direction;
        private bool isDefault;
        public static readonly BindableAttribute No = new BindableAttribute(false);
        public static readonly BindableAttribute Yes = new BindableAttribute(true);

        public BindableAttribute(bool bindable) : this(bindable, BindingDirection.OneWay)
        {
        }

        public BindableAttribute(BindableSupport flags) : this(flags, BindingDirection.OneWay)
        {
        }

        public BindableAttribute(bool bindable, BindingDirection direction)
        {
            this.bindable = bindable;
            this.direction = direction;
        }

        public BindableAttribute(BindableSupport flags, BindingDirection direction)
        {
            this.bindable = flags != BindableSupport.No;
            this.isDefault = flags == BindableSupport.Default;
            this.direction = direction;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is BindableAttribute)) && (((BindableAttribute) obj).Bindable == this.bindable)));
        }

        public override int GetHashCode()
        {
            return this.bindable.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            if (!this.Equals(Default))
            {
                return this.isDefault;
            }
            return true;
        }

        public bool Bindable
        {
            get
            {
                return this.bindable;
            }
        }

        public BindingDirection Direction
        {
            get
            {
                return this.direction;
            }
        }
    }
}

