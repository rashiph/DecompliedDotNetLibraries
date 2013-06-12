namespace System.Windows.Forms
{
    using System;
    using System.Collections;
    using System.Reflection;

    internal class DataGridViewIntLinkedList : IEnumerable
    {
        private int count;
        private DataGridViewIntLinkedListElement headElement;
        private DataGridViewIntLinkedListElement lastAccessedElement;
        private int lastAccessedIndex;

        public DataGridViewIntLinkedList()
        {
            this.lastAccessedIndex = -1;
        }

        public DataGridViewIntLinkedList(DataGridViewIntLinkedList source)
        {
            int count = source.Count;
            for (int i = 0; i < count; i++)
            {
                this.Add(source[i]);
            }
        }

        public void Add(int integer)
        {
            DataGridViewIntLinkedListElement element = new DataGridViewIntLinkedListElement(integer);
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

        public bool Contains(int integer)
        {
            int num = 0;
            DataGridViewIntLinkedListElement headElement = this.headElement;
            while (headElement != null)
            {
                if (headElement.Int == integer)
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

        public int IndexOf(int integer)
        {
            if (this.Contains(integer))
            {
                return this.lastAccessedIndex;
            }
            return -1;
        }

        public bool Remove(int integer)
        {
            DataGridViewIntLinkedListElement element = null;
            DataGridViewIntLinkedListElement headElement = this.headElement;
            while (headElement != null)
            {
                if (headElement.Int == integer)
                {
                    break;
                }
                element = headElement;
                headElement = headElement.Next;
            }
            if (headElement.Int != integer)
            {
                return false;
            }
            DataGridViewIntLinkedListElement next = headElement.Next;
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

        public void RemoveAt(int index)
        {
            DataGridViewIntLinkedListElement element = null;
            DataGridViewIntLinkedListElement headElement = this.headElement;
            while (index > 0)
            {
                element = headElement;
                headElement = headElement.Next;
                index--;
            }
            DataGridViewIntLinkedListElement next = headElement.Next;
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
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new DataGridViewIntLinkedListEnumerator(this.headElement);
        }

        public int Count
        {
            get
            {
                return this.count;
            }
        }

        public int HeadInt
        {
            get
            {
                return this.headElement.Int;
            }
        }

        public int this[int index]
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
                    return this.lastAccessedElement.Int;
                }
                DataGridViewIntLinkedListElement headElement = this.headElement;
                for (int i = index; i > 0; i--)
                {
                    headElement = headElement.Next;
                }
                this.lastAccessedElement = headElement;
                this.lastAccessedIndex = index;
                return headElement.Int;
            }
            set
            {
                if (index != this.lastAccessedIndex)
                {
                    int num1 = this[index];
                }
                this.lastAccessedElement.Int = value;
            }
        }
    }
}

