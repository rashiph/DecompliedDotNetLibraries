namespace System.Collections.Specialized
{
    using System;
    using System.Collections;

    public class StringEnumerator
    {
        private IEnumerator baseEnumerator;
        private IEnumerable temp;

        internal StringEnumerator(StringCollection mappings)
        {
            this.temp = mappings;
            this.baseEnumerator = this.temp.GetEnumerator();
        }

        public bool MoveNext()
        {
            return this.baseEnumerator.MoveNext();
        }

        public void Reset()
        {
            this.baseEnumerator.Reset();
        }

        public string Current
        {
            get
            {
                return (string) this.baseEnumerator.Current;
            }
        }
    }
}

