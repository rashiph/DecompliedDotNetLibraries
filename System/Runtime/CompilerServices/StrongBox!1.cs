namespace System.Runtime.CompilerServices
{
    using System;

    public class StrongBox<T> : IStrongBox
    {
        public T Value;

        public StrongBox()
        {
        }

        public StrongBox(T value)
        {
            this.Value = value;
        }

        object IStrongBox.Value
        {
            get
            {
                return this.Value;
            }
            set
            {
                this.Value = (T) value;
            }
        }
    }
}

