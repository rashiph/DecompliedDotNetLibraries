namespace System.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.All)]
    public sealed class ListBindableAttribute : Attribute
    {
        public static readonly ListBindableAttribute Default = Yes;
        private bool isDefault;
        private bool listBindable;
        public static readonly ListBindableAttribute No = new ListBindableAttribute(false);
        public static readonly ListBindableAttribute Yes = new ListBindableAttribute(true);

        public ListBindableAttribute(bool listBindable)
        {
            this.listBindable = listBindable;
        }

        public ListBindableAttribute(BindableSupport flags)
        {
            this.listBindable = flags != BindableSupport.No;
            this.isDefault = flags == BindableSupport.Default;
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }
            ListBindableAttribute attribute = obj as ListBindableAttribute;
            return ((attribute != null) && (attribute.ListBindable == this.listBindable));
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            if (!this.Equals(Default))
            {
                return this.isDefault;
            }
            return true;
        }

        public bool ListBindable
        {
            get
            {
                return this.listBindable;
            }
        }
    }
}

