namespace System.IdentityModel
{
    using System;
    using System.Reflection;

    internal abstract class IdentityModelStrings
    {
        protected IdentityModelStrings()
        {
        }

        public abstract int Count { get; }

        public abstract string this[int index] { get; }
    }
}

