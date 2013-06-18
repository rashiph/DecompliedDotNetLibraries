namespace Microsoft.JScript
{
    using System;
    using System.Collections;

    internal class ListEnumerator : IEnumerator
    {
        private int curr = -1;
        private ArrayList list;

        internal ListEnumerator(ArrayList list)
        {
            this.list = list;
        }

        public virtual bool MoveNext()
        {
            return (++this.curr < this.list.Count);
        }

        public virtual void Reset()
        {
            this.curr = -1;
        }

        public virtual object Current
        {
            get
            {
                return this.list[this.curr];
            }
        }
    }
}

