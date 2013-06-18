namespace System.Activities
{
    using System;

    internal abstract class LocationFactory
    {
        protected LocationFactory()
        {
        }

        public Location CreateLocation(ActivityContext context)
        {
            return this.CreateLocationCore(context);
        }

        protected abstract Location CreateLocationCore(ActivityContext context);
    }
}

