namespace System.Activities
{
    using System;
    using System.Runtime.CompilerServices;

    public abstract class LocationReference
    {
        protected LocationReference()
        {
        }

        public abstract Location GetLocation(ActivityContext context);

        internal int Id { get; set; }

        public string Name
        {
            get
            {
                return this.NameCore;
            }
        }

        protected abstract string NameCore { get; }

        public System.Type Type
        {
            get
            {
                return this.TypeCore;
            }
        }

        protected abstract System.Type TypeCore { get; }
    }
}

