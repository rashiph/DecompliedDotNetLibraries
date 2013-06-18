namespace System.Web.Services.Description
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class BasicProfileViolationEnumerator : IEnumerator<BasicProfileViolation>, IDisposable, IEnumerator
    {
        private int end;
        private int idx;
        private BasicProfileViolationCollection list;

        public BasicProfileViolationEnumerator(BasicProfileViolationCollection list)
        {
            this.list = list;
            this.idx = -1;
            this.end = list.Count - 1;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (this.idx >= this.end)
            {
                return false;
            }
            this.idx++;
            return true;
        }

        void IEnumerator.Reset()
        {
            this.idx = -1;
        }

        public BasicProfileViolation Current
        {
            get
            {
                return this.list[this.idx];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.list[this.idx];
            }
        }
    }
}

