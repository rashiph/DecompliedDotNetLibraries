namespace System.Runtime.Collections
{
    using System;
    using System.Runtime;

    internal abstract class ObjectCacheItem<T> where T: class
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected ObjectCacheItem()
        {
        }

        public abstract void ReleaseReference();
        public abstract bool TryAddReference();

        public abstract T Value { get; }
    }
}

