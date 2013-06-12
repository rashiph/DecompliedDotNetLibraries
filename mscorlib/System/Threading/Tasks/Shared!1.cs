namespace System.Threading.Tasks
{
    using System;

    internal class Shared<T>
    {
        internal T Value;

        internal Shared(T value)
        {
            this.Value = value;
        }
    }
}

