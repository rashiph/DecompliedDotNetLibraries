namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class FileLevelControlBuilderAttribute : Attribute
    {
        private Type builderType;
        public static readonly FileLevelControlBuilderAttribute Default = new FileLevelControlBuilderAttribute(null);

        public FileLevelControlBuilderAttribute(Type builderType)
        {
            this.builderType = builderType;
        }

        public override bool Equals(object obj)
        {
            return ((obj == this) || (((obj != null) && (obj is FileLevelControlBuilderAttribute)) && (((FileLevelControlBuilderAttribute) obj).BuilderType == this.builderType)));
        }

        public override int GetHashCode()
        {
            return this.builderType.GetHashCode();
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

