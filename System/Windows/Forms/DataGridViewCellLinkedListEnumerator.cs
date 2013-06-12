namespace System.Windows.Forms
{
    using System;
    using System.Collections;

    internal class DataGridViewCellLinkedListEnumerator : IEnumerator
    {
        private DataGridViewCellLinkedListElement current;
        private DataGridViewCellLinkedListElement headElement;
        private bool reset;

        public DataGridViewCellLinkedListEnumerator(DataGridViewCellLinkedListElement headElement)
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
                return this.current.DataGridViewCell;
            }
        }
    }
}

