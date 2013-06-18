namespace System.ServiceModel
{
    using System;
    using System.Reflection;

    internal abstract class ServiceModelStrings
    {
        protected ServiceModelStrings()
        {
        }

        public abstract int Count { get; }

        public abstract string this[int index] { get; }
    }
}

