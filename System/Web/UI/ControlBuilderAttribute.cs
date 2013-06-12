namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ControlBuilderAttribute : Attribute
    {
        private Type builderType;
        public static readonly ControlBuilderAttribute Default = new ControlBuilderAttribute(null);

        public ControlBuilderAttribute(Type builderType)
        {
            this.builderType = builderType;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is ControlBuilderAttribute)) && (((ControlBuilderAttribute) obj).BuilderType == this.builderType)));
        }

        public override int GetHashCode()
        {
            if (this.BuilderType == null)
            {
                return 0;
            }
            return this.BuilderType.GetHashCode();
        }

        public override bool IsDefaultAttribute()
        {
            return this.Equals(Default);
        }

        public Type BuilderType
        {
            get
            {
                return this.builderType;
            }
        }
    }
}

