namespace System.Runtime
{
    using System;
    using System.Runtime.InteropServices;

    internal class SignalGate<T> : SignalGate
    {
        private T result;

        public bool Signal(T result)
        {
            this.result = result;
            return base.Signal();
        }

        public bool Unlock(out T result)
        {
            if (base.Unlock())
            {
                result = this.result;
                return true;
            }
            result = default(T);
            return false;
        }
    }
}

