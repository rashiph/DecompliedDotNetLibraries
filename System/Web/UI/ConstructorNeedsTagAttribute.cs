namespace System.Web.UI
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class ConstructorNeedsTagAttribute : Attribute
    {
        private bool needsTag;

        public ConstructorNeedsTagAttribute()
        {
        }

        public ConstructorNeedsTagAttribute(bool needsTag)
        {
            this.needsTag = needsTag;
        }

        public bool NeedsTag
        {
            get
            {
                return this.needsTag;
            }
        }
    }
}

