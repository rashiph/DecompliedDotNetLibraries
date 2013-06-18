namespace System.Activities
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public abstract class LocationReferenceEnvironment
    {
        protected LocationReferenceEnvironment()
        {
        }

        public abstract IEnumerable<LocationReference> GetLocationReferences();
        public abstract bool IsVisible(LocationReference locationReference);
        public abstract bool TryGetLocationReference(string name, out LocationReference result);

        public LocationReferenceEnvironment Parent { get; protected set; }

        public abstract Activity Root { get; }
    }
}

