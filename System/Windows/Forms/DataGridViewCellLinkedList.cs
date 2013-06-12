namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class DataGridViewCellLinkedList : IEnumerable
    {
        private int count;
        private DataGridViewCellLinkedListElement headElement;
        private DataGridViewCellLinkedListElement lastAccessedElement;
        private int lastAccessedIndex = -1;

        public void Add(DataGridViewCell dataGridViewCell)
        {
            DataGridViewCellLinkedListElement element = new DataGridViewCellLinkedListElement(dataGridViewCell);
            if (this.headElement != null)
            {
                element.Next = this.headElement;
            }
            this.headElement = element;
            this.count++;
            this.lastAccessedElement = null;
            this.lastAccessedIndex = -1;
        }

        public void Clear()
        {
            this.lastAccessedElement = null;
            this.lastAccessedIndex = -1;
            this.headElement = null;
            this.count = 0;
        }

        public bool Contains(DataGridViewCell dataGridViewCell)
        {
            int num = 0;
            DataGridViewCellLinkedListElement headElement = this.headElement;
            while (headElement != null)
            {
                if (headElement.DataGridViewCell == dataGridViewCell)
                {
                    this.lastAccessedElement = headElement;
                    this.lastAccessedIndex = num;
                    return true;
                }
                headElement = headElement.Next;
                num++;
            }
            return false;
        }

        public bool Remove(DataGridViewCell dataGridViewCell)
        {
            DataGridViewCellLinkedListElement element = null;
            DataGridViewCellLinkedListElement headElement = this.headElement;
            while (headElement != null)
            {
                if (headElement.DataGridViewCell == dataGridViewCell)
                {
                    break;
                }
                element = headElement;
                headElement = headElement.Next;
            }
            if (headElement.DataGridViewCell != dataGridViewCell)
            {
                return false;
            }
            DataGridViewCellLinkedListElement next = headElement.Next;
            if (element == null)
            {
                this.headElement = next;
            }
            else
            {
                element.Next = next;
            }
            this.count--;
            this.lastAccessedElement = null;
            this.lastAccessedIndex = -1;
            return true;
        }

        public int RemoveAllCellsAtBand(bool column, int bandIndex)
        {
            int num = 0;
            DataGridViewCellLinkedListElement element = null;
            DataGridViewCellLinkedListElement headElement = this.headElement;
            while (headElement != null)
            {
                if ((column && (headElement.DataGridViewCell.ColumnIndex == bandIndex)) || (!column && (headElement.DataGridViewCell.RowIndex == bandIndex)))
                {
                    DataGridViewCellLinkedListElement next = headElement.Next;
                    if (element == null)
                    {
                        this.headElement = next;
                    }
                    else
                    {
                        element.Next = next;
                    }
                    headElement = next;
                    this.count--;
                    this.lastAccessedElement = null;
                    this.lastAccessedIndex = -1;
                    num++;
                }
                else
                {
                    element = headElement;
                    headElement = headElement.Next;
                }
            }
            return num;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataGridViewCellLinkedListEnumerator(this.headElement);
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public DataGridViewCell HeadCell
        {
            get
            {
                return this.headElement.DataGridViewCell;
            }
        }

        public DataGridViewCell this[int index]
        {
            get
            {
                if ((this.lastAccessedIndex != -1) && (index >= this.lastAccessedIndex))
                {
                    while (this.lastAccessedIndex < index)
                    {
                        this.lastAccessedElement = this.lastAccessedElement.Next;
                        this.lastAccessedIndex++;
                    }
                    return this.lastAccessedElement.DataGridViewCell;
                }
                DataGridViewCellLinkedListElement headElement = this.headElement;
                for (int i = index; i > 0; i--)
                {
                    headElement = headElement.Next;
                }
                this.lastAccessedElement = headElement;
                this.lastAccessedIndex = index;
                return headElement.DataGridViewCell;
            }
        }
    }
}

