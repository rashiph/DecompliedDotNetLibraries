namespace System.Net
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal class ListenerPrefixEnumerator : IEnumerator<string>, IDisposable, IEnumerator
    {
        private IEnumerator enumerator;

        internal ListenerPrefixEnumerator(IEnumerator enumerator)
        {
            this.enumerator = enumerator;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return this.enumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            this.enumerator.Reset();
        }

        public string Current
        {
            get
            {
                return (string) this.enumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.enumerator.Current;
            }
        }
    }
}

