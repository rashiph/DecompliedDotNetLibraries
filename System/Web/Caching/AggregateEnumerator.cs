namespace System.Web.Caching
{
    using System;
    using System.Collections;

    internal class AggregateEnumerator : IDictionaryEnumerator, IEnumerator
    {
        private IDictionaryEnumerator[] _enumerators;
        private int _iCurrent;

        internal AggregateEnumerator(IDictionaryEnumerator[] enumerators)
        {
            this._enumerators = enumerators;
        }

        public bool MoveNext()
        {
            while (true)
            {
                bool flag = this._enumerators[this._iCurrent].MoveNext();
                if (flag || (this._iCurrent == (this._enumerators.Length - 1)))
                {
                    return flag;
                }
                this._iCurrent++;
            }
        }

        public void Reset()
        {
            for (int i = 0; i <= this._iCurrent; i++)
            {
                this._enumerators[i].Reset();
            }
            this._iCurrent = 0;
        }

        public object Current
        {
            get
            {
                return this._enumerators[this._iCurrent].Current;
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                return this._enumerators[this._iCurrent].Entry;
            }
        }

        public object Key
        {
            get
            {
                return this._enumerators[this._iCurrent].Key;
            }
        }

        public object Value
        {
            get
            {
                return this._enumerators[this._iCurrent].Value;
            }
        }
    }
}

