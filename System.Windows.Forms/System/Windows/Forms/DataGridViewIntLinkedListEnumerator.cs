namespace System.Windows.Forms
{
    using System;
    using System.Collections;

    internal class DataGridViewIntLinkedListEnumerator : IEnumerator
    {
        private DataGridViewIntLinkedListElement current;
        private DataGridViewIntLinkedListElement headElement;
        private bool reset;

        public DataGridViewIntLinkedListEnumerator(DataGridViewIntLinkedListElement headElement)
        {
            this.headElement = headElement;
            this.reset = true;
        }

        bool IEnumerator.MoveNext()
        {
            if (this.reset)
            {
                this.current = this.headElement;
                this.reset = false;
            }
            else
            {
                this.current = this.current.Next;
            }
            return (this.current != null);
        }

        void IEnumerator.Reset()
        {
            this.reset = true;
            this.current = null;
        }

        object IEnumerator.Current
        {
            get
            {
                return this.current.Int;
            }
        }
    }
}

