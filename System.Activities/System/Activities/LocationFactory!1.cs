namespace System.Activities
{
    using System;

    internal abstract class LocationFactory<T> : LocationFactory
    {
        protected LocationFactory()
        {
        }

        public abstract Location<T> CreateLocation(ActivityContext context);
        protected override Location CreateLocationCore(ActivityContext context)
        {
            return this.CreateLocation(context);
        }
    }
}

