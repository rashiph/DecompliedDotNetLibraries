namespace System.Security.AccessControl
{
    using System;
    using System.Collections;

    public sealed class AceEnumerator : IEnumerator
    {
        private readonly GenericAcl _acl;
        private int _current;

        internal AceEnumerator(GenericAcl collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this._acl = collection;
            this.Reset();
        }

        public bool MoveNext()
        {
            this._current++;
            return (this._current < this._acl.Count);
        }

        public void Reset()
        {
            this._current = -1;
        }

        public GenericAce Current
        {
            get
            {
                return (((IEnumerator) this).Current as GenericAce);
            }
        }

        object IEnumerator.Current
        {
            get
            {
                if ((this._current == -1) || (this._current >= this._acl.Count))
                {
                    throw new InvalidOperationException(Environment.GetResourceString("Arg_InvalidOperationException"));
                }
                return this._acl[this._current];
            }
        }
    }
}

