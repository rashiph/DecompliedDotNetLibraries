namespace System.Runtime.CompilerServices
{
    using System;
    using System.Collections.Generic;

    [AttributeUsage(AttributeTargets.ReturnValue | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct | AttributeTargets.Class)]
    public sealed class DynamicAttribute : Attribute
    {
        private readonly bool[] _transformFlags;

        public DynamicAttribute()
        {
            this._transformFlags = new bool[] { true };
        }

        public DynamicAttribute(bool[] transformFlags)
        {
            if (transformFlags == null)
            {
                throw new ArgumentNullException("transformFlags");
            }
            this._transformFlags = transformFlags;
        }

        public IList<bool> TransformFlags
        {
            get
            {
                return Array.AsReadOnly<bool>(this._transformFlags);
            }
        }
    }
}

