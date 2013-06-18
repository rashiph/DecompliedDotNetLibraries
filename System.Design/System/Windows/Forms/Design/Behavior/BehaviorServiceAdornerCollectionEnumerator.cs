namespace System.Windows.Forms.Design.Behavior
{
    using System;
    using System.Collections;

    public class BehaviorServiceAdornerCollectionEnumerator : IEnumerator
    {
        private IEnumerator baseEnumerator;
        private IEnumerable temp;

        public BehaviorServiceAdornerCollectionEnumerator(BehaviorServiceAdornerCollection mappings)
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

        bool IEnumerator.MoveNext()
        {
            return this.baseEnumerator.MoveNext();
        }

        void IEnumerator.Reset()
        {
            this.baseEnumerator.Reset();
        }

        public Adorner Current
        {
            get
            {
                return (Adorner) this.baseEnumerator.Current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.baseEnumerator.Current;
            }
        }
    }
}

